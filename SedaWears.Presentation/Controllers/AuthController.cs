using MediatR;
using Microsoft.AspNetCore.Mvc;
using SedaWears.Application.Features.Auth.Commands;
using SedaWears.Application.Features.Auth.Models;

namespace SedaWears.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(ISender mediator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest? request)
    {
        await mediator.Send(new LoginCommand(request?.Email, request?.Password, request?.RememberMe ?? false));
        return Ok();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await mediator.Send(new LogoutCommand());
        return Ok(new { message = "Logged out successfully." });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest? request)
    {
        await mediator.Send(new RegisterCommand(
            request?.Email,
            request?.Password,
            request?.FirstName,
            request?.LastName,
            request?.Phone));
        return Ok(new { Message = "Registration successful. Please login to continue." });
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin(GoogleLoginRequest? request)
    {
        await mediator.Send(new LoginWithGoogleCommand(request?.IdToken));
        return Ok();
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
