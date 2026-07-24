using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Features.Shops.Models;
using SedaWears.Application.Features.Shops.Projections;
using SedaWears.Domain.Enums;
using ZiggyCreatures.Caching.Fusion;
using SedaWears.Application.Common;

namespace SedaWears.Application.Features.Shops.Queries;

public record GetShopQuery(int Id) : IRequest<ShopDto>;

public class GetShopHandler(
    IApplicationDbContext dbContext,
    IUserService userService,
    ICurrentUser currentUser,
    IOriginContext originContext,
    IOptions<OpeninaryConfig> configOptions,
    IFusionCache fusionCache) : IRequestHandler<GetShopQuery, ShopDto>
{
    public async Task<ShopDto> Handle(GetShopQuery request, CancellationToken ct)
    {
        var query = dbContext.Shops.AsNoTracking().Where(s => !s.IsDeleted).AsQueryable();

        if (originContext.OriginRole == UserRole.Customer)
        {
            return await fusionCache.GetOrSetAsync(
                CacheKeys.Shop(request.Id),
                async (cancellationToken) =>
                {
                    return await query
                        .Where(s => s.Id == request.Id && s.IsActive)
                        .ProjectToShop(configOptions.Value.BaseUrl)
                        .FirstOrDefaultAsync(cancellationToken) ?? throw new ShopNotFoundException();
                },
                CachePolicies.Shop,
                token: ct);
        }

        var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();

        if (!user.Roles.Contains(originContext.OriginRole))
            throw new ForbiddenException("You are not authorized to view this shop.");

        var userId = currentUser.Id;
        query = originContext.OriginRole switch
        {
            UserRole.Admin => query,
            UserRole.Owner => query.Where(s => s.Id != 1 && !s.IsDeleted && s.Owners.Any(o => o.UserId == userId)),
            UserRole.Manager => query.Where(s => s.Id != 1 && !s.IsDeleted && s.Managers.Any(m => m.UserId == userId)),
            _ => throw new ForbiddenException("You are not authorized to view this shop.")
        };

        return await query
            .Where(s => s.Id == request.Id)
            .ProjectToShop(configOptions.Value.BaseUrl)
            .FirstOrDefaultAsync(ct) ?? throw new ShopNotFoundException();
    }
}
