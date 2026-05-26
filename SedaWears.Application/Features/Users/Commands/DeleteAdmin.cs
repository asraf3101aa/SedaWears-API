using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace SedaWears.Application.Features.Users.Commands;

public record DeleteAdminCommand(int Id) : IRequest;

public class DeleteAdminValidator : AbstractValidator<DeleteAdminCommand>
{
    public DeleteAdminValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeleteAdminHandler(
    UserManager<User> userManager) : IRequestHandler<DeleteAdminCommand>
{
    public async Task Handle(DeleteAdminCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(request.Id.ToString())
            ?? throw new NotFoundException("User not found.");

        if (!await userManager.IsInRoleAsync(user, UserRole.Admin.ToString()))
            throw new NotFoundException("Admin not found.");

        var result = await userManager.RemoveFromRoleAsync(user, UserRole.Admin.ToString());
        if (!result.Succeeded) throw new BadRequestException(result.Errors.First().Description);

        var roles = await userManager.GetRolesAsync(user);
        if (roles.Count == 0)
        {
            var deleteResult = await userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded) throw new BadRequestException(deleteResult.Errors.First().Description);
        }

    }
}
