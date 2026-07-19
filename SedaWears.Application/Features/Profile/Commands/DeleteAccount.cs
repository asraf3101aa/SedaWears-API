using MediatR;
using Microsoft.AspNetCore.Identity;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Application.Common.Interfaces;

namespace SedaWears.Application.Features.Profile.Commands;

public record DeleteAccountCommand() : IRequest;

public class DeleteAccountCommandHandler(UserManager<User> userManager, ICurrentUser currentUser) :
    IRequestHandler<DeleteAccountCommand>
{
    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        var user = await userManager.FindByIdAsync(userId.ToString()) ?? throw new UserNotFoundException("User not found.");
        await userManager.DeleteAsync(user);
    }
}
