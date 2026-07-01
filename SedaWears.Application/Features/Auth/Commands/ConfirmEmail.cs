using SedaWears.Application.Features.Users.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;

namespace SedaWears.Application.Features.Auth.Commands;

public record ConfirmEmailCommand(int UserId, string Token) : IRequest;

public class ConfirmEmailHandler(UserManager<User> userManager) : IRequestHandler<ConfirmEmailCommand>
{
    public async Task Handle(ConfirmEmailCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString()) ?? throw new UserNotFoundException("User not found");
        var result = await userManager.ConfirmEmailAsync(user, request.Token);
        if (!result.Succeeded) throw new BadRequestException("Confirmation failed");
    }
}
