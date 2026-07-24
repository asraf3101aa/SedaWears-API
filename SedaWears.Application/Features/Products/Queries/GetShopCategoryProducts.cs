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

public record GetShopCategoryProductsQuery(
    int ShopId,
    int CategoryId,
    int PageNumber,
    int PageSize,
    ProductSortField SortBy,
    SortOrder SortOrder,
    string? Search) : IRequest<PaginatedList<ProductDto>>, IPaginatedQuery;

public class GetShopCategoryProductsValidator : PaginatedQueryValidator<GetShopCategoryProductsQuery> { }

public class GetShopCategoryProductsHandler(
    IApplicationDbContext dbContext,
    IUserService userService,
    ICurrentUser currentUser,
    IOriginContext originContext,
    IOptions<OpeninaryConfig> configOptions) : IRequestHandler<GetShopCategoryProductsQuery, PaginatedList<ProductDto>>
{
    public async Task<PaginatedList<ProductDto>> Handle(GetShopCategoryProductsQuery request, CancellationToken ct)
    {
        var shopQuery = dbContext.Shops.AsNoTracking().Where(s => s.Id == request.ShopId);

        var query = dbContext.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == request.CategoryId && p.Category.ShopId == request.ShopId);

        if (originContext.OriginRole == UserRole.Customer)
        {
            var shopExists = await shopQuery.AnyAsync(s => s.IsActive && !s.IsDeleted, ct);
            if (!shopExists)
                throw new ShopNotFoundException();

            var categoryExists = await dbContext.Categories
                .AnyAsync(c => c.Id == request.CategoryId && c.ShopId == request.ShopId && c.IsActive && !c.IsDeleted, ct);
            if (!categoryExists)
                throw new CategoryNotFoundException();

            query = query.Where(p => p.IsActive && !p.IsDeleted);
        }
        else
        {
            var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();

            if (!user.Roles.Contains(originContext.OriginRole))
                throw new ForbiddenException("You are not authorized to view these products.");

            var userId = currentUser.Id;
            bool shopExists = originContext.OriginRole switch
            {
                UserRole.Admin => await shopQuery.AnyAsync(s => !s.IsDeleted, ct),
                UserRole.Owner => await shopQuery.AnyAsync(s => s.Id != 1 && !s.IsDeleted && s.Owners.Any(o => o.UserId == userId), ct),
                UserRole.Manager => await shopQuery.AnyAsync(s => s.Id != 1 && !s.IsDeleted && s.Managers.Any(m => m.UserId == userId), ct),
                _ => false
            };

            if (!shopExists)
                throw new ShopNotFoundException();

            bool categoryExists = await dbContext.Categories
                .AnyAsync(c => c.Id == request.CategoryId && c.ShopId == request.ShopId && !c.IsDeleted, ct);

            if (!categoryExists)
                throw new CategoryNotFoundException();

            if (originContext.OriginRole != UserRole.Admin)
            {
                query = query.Where(p => !p.IsDeleted);
            }
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
