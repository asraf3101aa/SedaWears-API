using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SedaWears.Application.Features.Auth.Models;
using SedaWears.Application.Features.Auth.Commands;

namespace SedaWears.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(ISender mediator) : ControllerBase
{

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest? request)
    {
        var (userId, roles) = await mediator.Send(new LoginCommand(request?.Email?.Trim(), request?.Password, request?.RememberMe));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = request?.RememberMe ?? false,
            ExpiresUtc = (request?.RememberMe ?? false) ? DateTimeOffset.UtcNow.AddDays(30) : null
        };

        await HttpContext.SignInAsync(
            IdentityConstants.ApplicationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return Ok();
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin(GoogleLoginRequest? request)
    {
        var (userId, roles) = await mediator.Send(new LoginWithGoogleCommand(request?.IdToken, request?.RememberMe));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = request?.RememberMe ?? false,
            ExpiresUtc = (request?.RememberMe ?? false) ? DateTimeOffset.UtcNow.AddDays(30) : null
        };

        await HttpContext.SignInAsync(
            IdentityConstants.ApplicationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return Ok();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest? request)
    {
        await mediator.Send(
            new RegisterCommand(
            request?.Email?.Trim(),
            request?.Password,
            request?.FirstName?.Trim(),
            request?.LastName?.Trim(),
            request?.Phone?.Trim())
        );
        return Ok(new { Message = "Registration successful. Please login to continue." });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        return Ok(new { message = "Logged out successfully." });
    }

    [HttpPost("forgot-password")]

    public async Task<IActionResult> Forgot(ForgotPasswordRequest req)
    {
        await mediator.Send(new ForgotPasswordCommand(req.Email?.Trim()));
        return Ok(new { Message = "A reset password link has been sent to your email if it exists in our system." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> Reset(ResetPasswordRequest req)
    {
        await mediator.Send(new ResetPasswordCommand(req.Email?.Trim(), req.Token?.Trim(), req.NewPassword));
        return Ok();
    }
}

