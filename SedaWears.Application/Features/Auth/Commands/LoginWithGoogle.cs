using MediatR;
using Microsoft.AspNetCore.Identity;
using FluentValidation;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Auth.Commands;

public record LoginWithGoogleCommand(string? IdToken, bool? RememberMe) : IRequest<(int Id, IList<string> Roles)>;

public class LoginWithGoogleValidator : AbstractValidator<LoginWithGoogleCommand>
{
    public LoginWithGoogleValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("ID Token is required.");
    }
}

public class LoginWithGoogleHandler(
    UserManager<User> userManager,
    IGoogleAuthService googleAuthService,
    IOriginContext originContext) : IRequestHandler<LoginWithGoogleCommand, (int Id, IList<string> Roles)>
{
    public async Task<(int Id, IList<string> Roles)> Handle(LoginWithGoogleCommand request, CancellationToken ct)
    {
        var role = originContext.CurrentRole;
        
        if (role != UserRole.Customer)
        {
            throw new UnauthorizedAccessException("Google authentication is only available for customers.");
        }

        var payload = await googleAuthService.ValidateTokenAsync(request.IdToken!);
        
        if (payload == null)
        {
            throw new UnauthorizedAccessException("Invalid Google token.");
        }

        var identityUser = await userManager.FindByEmailAsync(payload.Email);

        if (identityUser == null)
        {
            // Auto-register new customer
            identityUser = new User
            {
                Email = payload.Email,
                UserName = payload.Email,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName,
                EmailConfirmed = true // Assuming Google emails are confirmed
            };
            
            var result = await userManager.CreateAsync(identityUser);
            if (!result.Succeeded) throw new BadRequestException(result.Errors.First().Description);

            await userManager.AddToRoleAsync(identityUser, nameof(UserRole.Customer));
        }
        else
        {
            if (!await userManager.IsInRoleAsync(identityUser, nameof(UserRole.Customer)))
            {
                throw new UnauthorizedAccessException("User is not a customer.");
            }
        }

        var roles = await userManager.GetRolesAsync(identityUser);
        return (identityUser.Id, roles);
    }
}
