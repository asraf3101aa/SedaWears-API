using Microsoft.Extensions.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Features.Products.Models;
using SedaWears.Application.Features.Products.Projections;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Common.Validators;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Products.Queries;

public record GetProductsQuery(
    int ShopId,
    int CategoryId,
    int PageNumber,
    int PageSize,
    ProductSortField SortBy,
    SortOrder SortOrder,
    string? Search) : IRequest<PaginatedList<ProductDto>>, IPaginatedQuery;

public class GetProductsValidator : PaginatedQueryValidator<GetProductsQuery> { }

public class GetProductsHandler(
    IApplicationDbContext dbContext,
    IUserService userService,
    ICurrentUser currentUser,
    IOriginContext originContext,
    IOptions<OpeninaryConfig> configOptions) : IRequestHandler<GetProductsQuery, PaginatedList<ProductDto>>
{
    public async Task<PaginatedList<ProductDto>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var shopExists = await dbContext.Shops.AnyAsync(s => s.Id == request.ShopId, ct);
        if (!shopExists)
            throw new ShopNotFoundException();

        if (request.ShopId == 1 && originContext.OriginRole is UserRole.Owner or UserRole.Manager)
            throw new ShopNotFoundException();

        var categoryExists = await dbContext.Categories
            .AnyAsync(c => c.Id == request.CategoryId && c.ShopId == request.ShopId, ct);
        if (!categoryExists)
            throw new CategoryNotFoundException();

        var query = dbContext.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == request.CategoryId && p.Category.ShopId == request.ShopId);

        if (originContext.OriginRole == UserRole.Customer)
        {
            query = query.Where(p => p.IsActive && p.Category.IsActive && p.Category.Shop.IsActive);
        }
        else
        {
            var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();

            if (!user.Roles.Contains(originContext.OriginRole))
                throw new ForbiddenException("You are not authorized to view these products.");

            var userId = currentUser.Id;
            query = originContext.OriginRole switch
            {
                UserRole.Admin => query,
                UserRole.Owner => query.Where(p => p.Category.Shop.Owners.Any(o => o.UserId == userId)),
                UserRole.Manager => query.Where(p => p.Category.Shop.Managers.Any(m => m.UserId == userId)),
                _ => throw new ForbiddenException("You are not authorized to view these products.")
            };
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.Trim();
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{searchTerm}%"));
        }

        var isDescending = request.SortOrder == SortOrder.Desc;
        query = request.SortBy switch
        {
            ProductSortField.Name => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            ProductSortField.Price => isDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            ProductSortField.CreatedAt => isDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync(ct);
        var products = await query.Skip((request.PageNumber - 1) * request.PageSize)
                                  .Take(request.PageSize)
                                  .ProjectToProduct(configOptions.Value.BaseUrl)
                                  .ToListAsync(ct);

        return new PaginatedList<ProductDto>(products, totalCount, request.PageNumber, request.PageSize);
    }
}
