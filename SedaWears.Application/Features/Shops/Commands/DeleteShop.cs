using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;

namespace SedaWears.Application.Features.Shops.Commands;

public record DeleteShopCommand(int Id) : IRequest;

public class DeleteShopHandler(IApplicationDbContext dbContext, IUserService userService) : IRequestHandler<DeleteShopCommand>
{
    public async Task Handle(DeleteShopCommand request, CancellationToken ct)
    {
        var shop = await dbContext.Shops.FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new ShopNotFoundException();

        var affectedUserIds = await dbContext.ShopOwners.Where(so => so.ShopId == request.Id).Select(so => so.UserId)
            .Union(dbContext.ShopManagers.Where(sm => sm.ShopId == request.Id).Select(sm => sm.UserId))
            .ToListAsync(ct);

        dbContext.Shops.Remove(shop);
        await dbContext.SaveChangesAsync(ct);

        foreach (var userId in affectedUserIds)
            await userService.SyncMemberRoleAsync(userId, ct);
    }
}
