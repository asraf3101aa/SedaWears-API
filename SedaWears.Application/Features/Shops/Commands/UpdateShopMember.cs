using MediatR;
using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;
using SedaWears.Application.Common;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;

namespace SedaWears.Application.Features.Shops.Commands;

public record UpdateShopMemberCommand(int ShopId, int UserId, string? FirstName, string? LastName) : IRequest
{
    public string? FirstName { get; init; } = FirstName?.Trim();
    public string? LastName { get; init; } = LastName?.Trim();
}

public class UpdateShopMemberHandler(IApplicationDbContext dbContext, IFusionCache fusionCache) : IRequestHandler<UpdateShopMemberCommand>
{
    public async Task Handle(UpdateShopMemberCommand request, CancellationToken ct)
    {
        var user = await dbContext.ShopOwners
            .Where(so => so.ShopId == request.ShopId && so.UserId == request.UserId)
            .Select(so => so.User)
            .FirstOrDefaultAsync(ct);

        user ??= await dbContext.ShopManagers
            .Where(sm => sm.ShopId == request.ShopId && sm.UserId == request.UserId)
            .Select(sm => sm.User)
            .FirstOrDefaultAsync(ct);

        if (user == null)
            throw new ShopNotFoundException();

        user.FirstName = request.FirstName!;
        user.LastName = request.LastName!;

        await dbContext.SaveChangesAsync(ct);
        await fusionCache.RemoveAsync(CacheKeys.User(request.UserId), token: ct);
    }
}
