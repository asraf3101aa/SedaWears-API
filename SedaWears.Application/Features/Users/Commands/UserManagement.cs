using MediatR;
using FluentValidation;
using SedaWears.Application.Common.Validators;
using Microsoft.AspNetCore.Identity;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Application.Features.Users.Projections;



using Microsoft.EntityFrameworkCore;

namespace SedaWears.Application.Features.Users.Commands;

public record UpdateUserCommand(int Id, string? FirstName, string? LastName) : IRequest
{
    public string? FirstName { get; init; } = FirstName?.Trim();
    public string? LastName { get; init; } = LastName?.Trim();
}

public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.");
    }
}

public class UpdateUserHandler(UserManager<User> userManager) : IRequestHandler<UpdateUserCommand>
{
    public async Task Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var role = UserRole.Customer;
        var user = await userManager.FindByIdAsync(request.Id.ToString());
        
        if (user == null || !await userManager.IsInRoleAsync(user, role.ToString()))
            throw new NotFoundException($"{role} not found.");

        user.FirstName = request.FirstName!;
        user.LastName = request.LastName!;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded) throw new BadRequestException(result.Errors.First().Description);
    }
}
