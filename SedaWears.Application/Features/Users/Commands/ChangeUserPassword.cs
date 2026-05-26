using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Validators;

namespace SedaWears.Application.Features.Users.Commands;

public record ChangeUserPasswordCommand(string? OldPassword, string? NewPassword) : IRequest;

public class ChangeUserPasswordValidator : AbstractValidator<ChangeUserPasswordCommand>
{
    public ChangeUserPasswordValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage("Old password is required.");

        RuleFor(x => x.NewPassword!)
            .Password();
    }
}

public class ChangeUserPasswordHandler(
    UserManager<User> userManager,
    IOriginContext originContext,
    ICurrentUser currentUser) : IRequestHandler<ChangeUserPasswordCommand>
{
    public async Task Handle(ChangeUserPasswordCommand request, CancellationToken ct)
    {
        var userId = currentUser.Id!.Value;
        var role = originContext.CurrentRole;

        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user == null || !await userManager.IsInRoleAsync(user, role.ToString()))
            throw new UnauthorizedAccessException();

        var result = await userManager.ChangePasswordAsync(user, request.OldPassword!, request.NewPassword!);

        if (!result.Succeeded) throw new BadRequestException(result.Errors.First().Description);
    }
}
