using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;

namespace SedaWears.Application.Features.Users.Commands;

public record DeleteInvitedAdminCommand(int Id) : IRequest;

public class DeleteInvitedAdminHandler(
    IApplicationDbContext dbContext) : IRequestHandler<DeleteInvitedAdminCommand>
{
    public async Task Handle(DeleteInvitedAdminCommand request, CancellationToken ct)
    {
        var invitation = await dbContext.InvitedAdmins
            .FirstOrDefaultAsync(ia => ia.Id == request.Id, ct)
            ?? throw new InvitationNotFoundException("Admin invitation not found.");

        dbContext.InvitedAdmins.Remove(invitation);
        await dbContext.SaveChangesAsync(ct);
    }
}
