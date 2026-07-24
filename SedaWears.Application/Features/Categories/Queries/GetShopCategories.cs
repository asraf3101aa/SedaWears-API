using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Features.Categories.Models;
using SedaWears.Application.Features.Categories.Projections;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Categories.Queries;

public record GetShopCategoriesQuery(
    int ShopId,
    CategorySortField SortBy,
    SortOrder SortOrder,
    string? Search) : IRequest<List<CategoryDto>>;

public class GetShopCategoriesHandler(
    IApplicationDbContext dbContext,
    IUserService userService,
    ICurrentUser currentUser,
    IOriginContext originContext) : IRequestHandler<GetShopCategoriesQuery, List<CategoryDto>>
{
    public async Task<List<CategoryDto>> Handle(GetShopCategoriesQuery request, CancellationToken ct)
    {
        var shopQuery = dbContext.Shops.AsNoTracking().Where(s => s.Id == request.ShopId);

        var query = dbContext.Categories
            .AsNoTracking()
            .Where(c => c.ShopId == request.ShopId);

        if (originContext.OriginRole == UserRole.Customer)
        {
            var shopExists = await shopQuery.AnyAsync(s => s.IsActive && !s.IsDeleted, ct);
            if (!shopExists)
                throw new ShopNotFoundException();

            query = query.Where(c => c.IsActive && !c.IsDeleted).OrderBy(c => c.DisplayOrder);
        }

        else
        {
            var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();

            if (!user.Roles.Contains(originContext.OriginRole))
                throw new ForbiddenException("You are not authorized to view these categories.");

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

            if (originContext.OriginRole != UserRole.Admin)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchTerm = request.Search.Trim();
                query = query.Where(c => EF.Functions.ILike(c.Name, $"%{searchTerm}%"));
            }

            var isDescending = request.SortOrder == SortOrder.Desc;
            query = request.SortBy switch
            {
                CategorySortField.Name => isDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                CategorySortField.IsActive => isDescending ? query.OrderByDescending(c => c.IsActive) : query.OrderBy(c => c.IsActive),
                CategorySortField.DisplayOrder => isDescending ? query.OrderByDescending(c => c.DisplayOrder) : query.OrderBy(c => c.DisplayOrder),
                _ => query.OrderByDescending(c => c.DisplayOrder)
            };
        }

        return await query
            .ProjectToCategory()
            .ToListAsync(ct);
    }
}
