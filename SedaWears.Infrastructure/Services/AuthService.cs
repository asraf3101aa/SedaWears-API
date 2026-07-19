using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;

namespace SedaWears.Infrastructure.Services;

public class AuthService(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IGoogleAuthService googleAuthService,
    IHostEnvironment env) : IAuthService
{
    public async Task SignInAsync(string email, string password, bool rememberMe, UserRole role, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null || !await userManager.IsInRoleAsync(user, role.ToString()))
            throw new UnauthorizedAccessException("Incorrect email or password.");

        var result = await signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: env.IsProduction());

        if (result.IsLockedOut)
            throw new UnauthorizedAccessException("Account is locked out. Please try again later.");

        if (!result.Succeeded)
            throw new UnauthorizedAccessException("Incorrect email or password.");
    }

    public async Task SignInWithGoogleAsync(string idToken, CancellationToken ct = default)
    {
        var payload = await googleAuthService.ValidateTokenAsync(idToken)
            ?? throw new UnauthorizedAccessException("Invalid Google token.");

        var user = await userManager.FindByEmailAsync(payload.Email);

        if (user == null)
        {
            user = new User
            {
                Email = payload.Email,
                UserName = payload.Email,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                throw new BadRequestException(createResult.Errors.First().Description);

            await userManager.AddToRoleAsync(user, nameof(UserRole.Customer));
        }
        else if (!await userManager.IsInRoleAsync(user, nameof(UserRole.Customer)))
        {
            var addToRoleResult = await userManager.AddToRoleAsync(user, nameof(UserRole.Customer));
            if (!addToRoleResult.Succeeded)
                throw new BadRequestException(addToRoleResult.Errors.First().Description);
        }

        await signInManager.SignInAsync(user, isPersistent: false);
    }

    public async Task SignOutAsync() => await signInManager.SignOutAsync();
}
