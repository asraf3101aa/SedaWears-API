using MediatR;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace SedaWears.Application.Features.Shops.Commands;

public record DeleteInvitedShopMemberCommand(int ShopId, int InvitationId, UserRole Role) : IRequest;

public class DeleteInvitedShopMemberHandler(IApplicationDbContext dbContext) : IRequestHandler<DeleteInvitedShopMemberCommand>
{
    public async Task Handle(DeleteInvitedShopMemberCommand request, CancellationToken ct)
    {
        if (request.Role == UserRole.Owner)
        {
            var invitation = await dbContext.InvitedShopOwners
                .FirstOrDefaultAsync(iso => iso.ShopId == request.ShopId && iso.Id == request.InvitationId, ct)
                ?? throw new InvitationNotFoundException("Shop owner invitation not found.");

            dbContext.InvitedShopOwners.Remove(invitation);
        }
        else if (request.Role == UserRole.Manager)
        {
            var invitation = await dbContext.InvitedShopManagers
                .FirstOrDefaultAsync(ism => ism.ShopId == request.ShopId && ism.Id == request.InvitationId, ct)
                ?? throw new InvitationNotFoundException("Shop manager invitation not found.");

            dbContext.InvitedShopManagers.Remove(invitation);
        }
        else
        {
            throw new BadRequestException("Invalid role for shop invitation deletion.");
        }

        await dbContext.SaveChangesAsync(ct);
    }
}
