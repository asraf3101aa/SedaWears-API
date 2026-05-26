using MediatR;
using Microsoft.AspNetCore.Identity;
using FluentValidation;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Auth.Commands;

public record LoginCommand(string? Email, string? Password, bool? RememberMe) : IRequest<(int Id, IList<string> Roles)>;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class LoginHandler(
    UserManager<User> userManager,
    IOriginContext originContext) : IRequestHandler<LoginCommand, (int Id, IList<string> Roles)>
{
    public async Task<(int Id, IList<string> Roles)> Handle(LoginCommand request, CancellationToken ct)
    {
        var role = originContext.CurrentRole;
        var identityUser = await userManager.FindByEmailAsync(request.Email!);

        if (identityUser == null || !await userManager.IsInRoleAsync(identityUser, role.ToString()))
        {
            throw new UnauthorizedAccessException("Incorrect email or password.");
        }

        if (!await userManager.CheckPasswordAsync(identityUser, request.Password!))
        {
            throw new UnauthorizedAccessException("Incorrect email or password.");
        }

        var roles = await userManager.GetRolesAsync(identityUser);
        return (identityUser.Id, roles);
    }
}
