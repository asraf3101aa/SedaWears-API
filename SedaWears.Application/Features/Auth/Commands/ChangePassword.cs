using MediatR;
using Microsoft.AspNetCore.Identity;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;

using FluentValidation;
using SedaWears.Application.Common.Validators;
using SedaWears.Application.Common.Interfaces;

namespace SedaWears.Application.Features.Auth.Commands;

public record ChangePasswordCommand(string? CurrentPassword, string? NewPassword) : IRequest;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword!)
            .Password();
    }
}

public class ChangePasswordHandler(UserManager<User> userManager, ICurrentUser currentUser) : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var userId = currentUser.Id;
        var user = await userManager.FindByIdAsync(userId.ToString()) ?? throw new UserNotFoundException("User not found.");
        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword!, request.NewPassword!);
        if (!result.Succeeded) throw new BadRequestException(result.Errors.First().Description);
    }
}
