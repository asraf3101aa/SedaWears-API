using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;

namespace SedaWears.Application.Features.Shops.Commands;

public record DeleteShopMemberCommand(int ShopId, int UserId) : IRequest;

public class DeleteShopMemberHandler(IApplicationDbContext dbContext, IUserService userService) : IRequestHandler<DeleteShopMemberCommand>
{
    public async Task Handle(DeleteShopMemberCommand request, CancellationToken ct)
    {
        var owner = await dbContext.ShopOwners.FirstOrDefaultAsync(so => so.ShopId == request.ShopId && so.UserId == request.UserId, ct);
        var manager = owner == null
            ? await dbContext.ShopManagers.FirstOrDefaultAsync(sm => sm.ShopId == request.ShopId && sm.UserId == request.UserId, ct)
            : null;

        if (owner == null && manager == null)
            throw new ShopNotFoundException();

        if (owner != null) dbContext.ShopOwners.Remove(owner);
        if (manager != null) dbContext.ShopManagers.Remove(manager);

        await dbContext.SaveChangesAsync(ct);

        await userService.SyncMemberRoleAsync(request.UserId, ct);
    }
}
