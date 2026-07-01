using MediatR;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using SedaWears.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using SedaWears.Domain.Entities;

namespace SedaWears.Application.Features.Shops.Commands;

public record DeleteShopMemberCommand(int ShopId, int UserId) : IRequest;

public class DeleteShopMemberHandler(IApplicationDbContext dbContext, UserManager<User> userManager) : IRequestHandler<DeleteShopMemberCommand>
{
    public async Task Handle(DeleteShopMemberCommand request, CancellationToken ct)
    {
        User? user = null;

        var owner = await dbContext.ShopOwners
            .Include(so => so.User)
            .FirstOrDefaultAsync(so => so.ShopId == request.ShopId && so.UserId == request.UserId, ct);

        if (owner != null)
        {
            user = owner.User;
            dbContext.ShopOwners.Remove(owner);
        }
        else
        {
            var manager = await dbContext.ShopManagers
                .Include(sm => sm.User)
                .FirstOrDefaultAsync(sm => sm.ShopId == request.ShopId && sm.UserId == request.UserId, ct);

            if (manager != null)
            {
                user = manager.User;
                dbContext.ShopManagers.Remove(manager);
            }
        }

        if (user == null)
            throw new ShopNotFoundException();

        await dbContext.SaveChangesAsync(ct);

        // Check if the user is associated with any OTHER shops
        var hasOtherMemberships = await dbContext.ShopOwners.AnyAsync(so => so.UserId == request.UserId, ct)
            || await dbContext.ShopManagers.AnyAsync(sm => sm.UserId == request.UserId, ct);

        if (!hasOtherMemberships)
        {
            // Only auto-delete the user if their primary role is shop-related (Manager/Owner)
            // This prevents accidental deletion of Admins or Customers who have shop memberships
            var isManager = await userManager.IsInRoleAsync(user, UserRole.Manager.ToString());
            var isOwner = await userManager.IsInRoleAsync(user, UserRole.Owner.ToString());

            if (isManager || isOwner)
            {
                await userManager.DeleteAsync(user);
            }
        }
    }
}
