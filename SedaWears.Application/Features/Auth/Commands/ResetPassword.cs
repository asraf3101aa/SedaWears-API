using MediatR;
using FluentValidation;
using SedaWears.Application.Common.Validators;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Entities;

namespace SedaWears.Application.Features.Auth.Commands;

public record ResetPasswordCommand(string? Email, string? Token, string? NewPassword) : IRequest;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");

        RuleFor(x => x.NewPassword!)
            .Password();
    }
}

public class ResetPasswordHandler(
    UserManager<User> userManager,
    IOriginContext originContext) : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var role = originContext.CurrentRole;
        var usersInRole = await userManager.GetUsersInRoleAsync(role.ToString());
        var user = usersInRole.FirstOrDefault(u => u.Email == request.Email) ?? throw new BadRequestException("Invalid email or token.");
        var result = await userManager.ResetPasswordAsync(user, request.Token!, request.NewPassword!);
        if (!result.Succeeded)
        {
            throw new BadRequestException("Invalid email or token.");
        }
    }
}
