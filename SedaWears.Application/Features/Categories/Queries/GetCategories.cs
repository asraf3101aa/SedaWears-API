using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Features.Categories.Models;
using SedaWears.Application.Features.Categories.Projections;
using SedaWears.Application.Common.Validators;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Categories.Queries;

public record GetCategoriesQuery(
    int ShopId,
    int PageNumber,
    int PageSize,
    CategorySortBy SortBy,
    SortOrder SortOrder,
    string? Search) : IRequest<PaginatedList<CategoryDto>>, IPaginatedQuery;

public class GetCategoriesValidator : PaginatedQueryValidator<GetCategoriesQuery> { }

public class GetCategoriesHandler(
    IApplicationDbContext dbContext,
    IUserService userService,
    ICurrentUser currentUser,
    IOriginContext originContext) : IRequestHandler<GetCategoriesQuery, PaginatedList<CategoryDto>>
{
    public async Task<PaginatedList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        var shopExists = await dbContext.Shops.AsNoTracking().AnyAsync(s => s.Id == request.ShopId, ct);
        if (!shopExists)
            throw new ShopNotFoundException();

        if (request.ShopId == 1 && originContext.OriginRole is UserRole.Owner or UserRole.Manager)
            throw new ShopNotFoundException();

        var query = dbContext.Categories
            .AsNoTracking()
            .Where(c => c.ShopId == request.ShopId);

        if (originContext.OriginRole == UserRole.Customer)
        {
            query = query.Where(c => c.Shop.IsActive).OrderBy(c => c.DisplayOrder);
        }
        else
        {
            var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();

            if (!user.Roles.Contains(originContext.OriginRole))
                throw new ForbiddenException("You are not authorized to view these categories.");

            var userId = currentUser.Id;
            query = originContext.OriginRole switch
            {
                UserRole.Admin => query,
                UserRole.Owner => query.Where(c => c.Shop.Owners.Any(o => o.UserId == userId)),
                UserRole.Manager => query.Where(c => c.Shop.Managers.Any(m => m.UserId == userId)),
                _ => throw new ForbiddenException("You are not authorized to view these categories.")
            };

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchTerm = request.Search.Trim();
                query = query.Where(c => EF.Functions.ILike(c.Name, $"%{searchTerm}%"));
            }

            var isDescending = request.SortOrder == SortOrder.Desc;
            query = request.SortBy switch
            {
                CategorySortBy.Name => isDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                CategorySortBy.IsActive => isDescending ? query.OrderByDescending(c => c.IsActive) : query.OrderBy(c => c.IsActive),
                CategorySortBy.DisplayOrder => isDescending ? query.OrderByDescending(c => c.DisplayOrder) : query.OrderBy(c => c.DisplayOrder),
                _ => query.OrderByDescending(c => c.DisplayOrder)
            };
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToCategory()
            .ToListAsync(ct);

        return new PaginatedList<CategoryDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
