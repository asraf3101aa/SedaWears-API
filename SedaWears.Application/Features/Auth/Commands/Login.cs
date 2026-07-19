using MediatR;
using FluentValidation;
using SedaWears.Application.Common.Interfaces;

namespace SedaWears.Application.Features.Auth.Commands;

public record LoginCommand(string? Email, string? Password, bool RememberMe) : IRequest;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class LoginHandler(IAuthService authService, IOriginContext originContext) : IRequestHandler<LoginCommand>
{
    public async Task Handle(LoginCommand request, CancellationToken ct)
    {
        await authService.SignInAsync(request.Email!.Trim(), request.Password!, request.RememberMe, originContext.OriginRole, ct);
    }
}
