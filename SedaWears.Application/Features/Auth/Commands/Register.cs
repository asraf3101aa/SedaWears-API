using MediatR;
using Microsoft.AspNetCore.Identity;
using FluentValidation;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Validators;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Auth.Commands;

public record RegisterCommand(string? Email, string? Password, string? FirstName, string? LastName, string? Phone) : IRequest
{
    public string? Email { get; init; } = Email?.Trim();
    public string? FirstName { get; init; } = FirstName?.Trim();
    public string? LastName { get; init; } = LastName?.Trim();
    public string? Phone { get; init; } = Phone?.Trim();
}

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator(UserManager<User> userManager)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.")
            .MustAsync(async (email, ct) =>
            {
                if (string.IsNullOrEmpty(email)) return true;
                var user = await userManager.FindByEmailAsync(email);
                if (user == null) return true;
                return !await userManager.IsInRoleAsync(user, nameof(UserRole.Customer));
            }).WithMessage("Email address already registered.");

        RuleFor(x => x.Password!)
            .Password();

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.");
    }
}

public class RegisterHandler(
    IUserService userService) : IRequestHandler<RegisterCommand>
{
    public async Task Handle(RegisterCommand request, CancellationToken ct)
    {
        var email = request.Email!;
        var existingUser = await userService.FindByEmailAsync(email);

        if (existingUser != null)
        {
            await userService.AddToRoleAsync(existingUser, nameof(UserRole.Customer));
            return;
        }

        var user = new User
        {
            Email = email,
            UserName = email,
            FirstName = request.FirstName!,
            LastName = request.LastName!,
            PhoneNumber = request.Phone!
        };

        await userService.CreateUserAsync(user, request.Password!);
        await userService.AddToRoleAsync(user, nameof(UserRole.Customer));
    }
}
