using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;
using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Application.Features.Profile.Commands;

public record UpdateProfileCommand(string? FirstName, string? LastName, string? Phone) : IRequest
{
    public string? FirstName { get; init; } = FirstName?.Trim();
    public string? LastName { get; init; } = LastName?.Trim();
    public string? Phone { get; init; } = Phone?.Trim();
}

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(v => v.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.");

        RuleFor(v => v.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");

        RuleFor(v => v.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[0-9]{7,15}$").WithMessage("A valid phone number is required.");
    }
}

public class UpdateProfileCommandHandler(
    IApplicationDbContext dbContext,
    UserManager<User> userManager,
    ICurrentUser currentUser,
    IFusionCache fusionCache) : IRequestHandler<UpdateProfileCommand>
{
    public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        var role = UserRole.Customer;

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null || !await userManager.IsInRoleAsync(user, role.ToString()))
        {
            throw new UnauthorizedAccessException();
        }

        var rowsAffected = await dbContext.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.FirstName, request.FirstName)
                .SetProperty(u => u.LastName, request.LastName)
                .SetProperty(u => u.PhoneNumber, request.Phone),
                cancellationToken);

        if (rowsAffected == 0)
        {
            throw new UserNotFoundException("User not found.");
        }

        await fusionCache.RemoveAsync(CacheKeys.User(userId), token: cancellationToken);
    }
}
