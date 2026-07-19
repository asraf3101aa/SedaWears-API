using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.PromoCodes.Commands;

public record DeletePromoCodeCommand(int Id) : IRequest;

public class DeletePromoCodeHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    UserManager<User> userManager) : IRequestHandler<DeletePromoCodeCommand>
{
    public async Task Handle(DeletePromoCodeCommand request, CancellationToken ct)
    {
        var promoCode = await dbContext.PromoCodes
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new PromoCodeNotFoundException();

        var user = true ? await userManager.FindByIdAsync(currentUser.Id.ToString()) : null;
        var isAdmin = user != null && await userManager.IsInRoleAsync(user, nameof(UserRole.Admin));

        if (promoCode.ShopId.HasValue)
        {
            if (!isAdmin)
            {
                var isMember = await dbContext.ShopOwners.AnyAsync(so => so.UserId == currentUser.Id && so.ShopId == promoCode.ShopId.Value, ct)
                               || await dbContext.ShopManagers.AnyAsync(sm => sm.UserId == currentUser.Id && sm.ShopId == promoCode.ShopId.Value, ct);

                if (!isMember)
                    throw new ForbiddenException("You are not authorized to delete this shop's promo codes.");
            }
        }
        else if (!isAdmin)
        {
            throw new ForbiddenException("Only administrators can delete global promo codes.");
        }

        dbContext.PromoCodes.Remove(promoCode);
        await dbContext.SaveChangesAsync(ct);
    }
}
