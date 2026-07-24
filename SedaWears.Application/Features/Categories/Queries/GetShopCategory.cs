using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Features.Categories.Models;
using SedaWears.Application.Features.Categories.Projections;
using SedaWears.Application.Common;
using SedaWears.Domain.Enums;
using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Application.Features.Categories.Queries;

public record GetShopCategoryQuery(int Id, int ShopId) : IRequest<CategoryDto>;

public class GetShopCategoryHandler(
    IApplicationDbContext dbContext,
    IUserService userService,
    ICurrentUser currentUser,
    IOriginContext originContext,
    IFusionCache fusionCache) : IRequestHandler<GetShopCategoryQuery, CategoryDto>
{
    public async Task<CategoryDto> Handle(GetShopCategoryQuery request, CancellationToken ct)
    {
        var shopQuery = dbContext.Shops.AsNoTracking().Where(s => s.Id == request.ShopId);

        var query = dbContext.Categories
            .AsNoTracking()
            .Where(c => c.Id == request.Id && c.ShopId == request.ShopId);

        if (originContext.OriginRole == UserRole.Customer)
        {
            return await fusionCache.GetOrSetAsync(
                CacheKeys.Category(request.Id),
                async (cancellationToken) =>
                {
                    var shopExists = await shopQuery.AnyAsync(s => s.IsActive && !s.IsDeleted, cancellationToken);
                    if (!shopExists)
                        throw new ShopNotFoundException();

                    return await query
                        .Where(c => c.IsActive && !c.IsDeleted)
                        .ProjectToCategory()
                        .FirstOrDefaultAsync(cancellationToken) ?? throw new CategoryNotFoundException();
                },
                CachePolicies.Category,
                token: ct);
        }
        else
        {
            var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();

            if (!user.Roles.Contains(originContext.OriginRole))
                throw new ForbiddenException("You are not authorized to view this category.");

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
        }

        return await query
            .ProjectToCategory()
            .FirstOrDefaultAsync(ct) ?? throw new CategoryNotFoundException();
    }
}
