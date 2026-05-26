using MediatR;
using FluentValidation;
using SedaWears.Application.Common.Validators;
using Microsoft.AspNetCore.Identity;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Application.Features.Users.Projections;

using SedaWears.Application.Common.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace SedaWears.Application.Features.Users.Commands;

public record UpdateUserCommand(int Id, string? FirstName, string? LastName) : IRequest;

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

public class UpdateUserHandler(UserManager<User> userManager, IOriginContext originContext) : IRequestHandler<UpdateUserCommand>
{
    public async Task Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var role = originContext.CurrentRole;
        var user = await userManager.FindByIdAsync(request.Id.ToString());
        
        if (user == null || !await userManager.IsInRoleAsync(user, role.ToString()))
            throw new NotFoundException($"{role} not found.");

        user.FirstName = request.FirstName!;
        user.LastName = request.LastName!;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded) throw new BadRequestException(result.Errors.First().Description);
    }
}
