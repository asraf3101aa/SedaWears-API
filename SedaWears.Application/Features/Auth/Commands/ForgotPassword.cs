using Microsoft.Extensions.Options;
using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Settings;
using System.Web;

namespace SedaWears.Application.Features.Auth.Commands;

public record ForgotPasswordCommand(string? Email) : IRequest
{
    public string? Email { get; init; } = Email?.Trim();
}

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.");
    }
}

public class ForgotPasswordHandler(
    UserManager<User> userManager,
    IEmailService emailService,
    IOptions<HostUrlsConfig> hostUrlsConfigOptions) : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var role = UserRole.Customer;
        var usersInRole = await userManager.GetUsersInRoleAsync(role.ToString());
        var user = usersInRole.FirstOrDefault(u => u.Email == request.Email);

        if (user == null) return;

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var frontendUrl = role switch
        {
            UserRole.Admin => hostUrlsConfigOptions.Value.Admin,
            UserRole.Manager => hostUrlsConfigOptions.Value.Manager,
            UserRole.Owner => hostUrlsConfigOptions.Value.Owner,
            _ => hostUrlsConfigOptions.Value.Customer
        };
        var url = $"{frontendUrl}/reset-password?email={user.Email}&token={HttpUtility.UrlEncode(token)}";

        await emailService.SendForgotPasswordEmailAsync(user.Email!, url);
    }
}
