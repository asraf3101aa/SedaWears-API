using MediatR;
using FluentValidation;
using SedaWears.Application.Common.Interfaces;

namespace SedaWears.Application.Features.Auth.Commands;

public record LoginWithGoogleCommand(string? IdToken) : IRequest
{
    public string? IdToken { get; init; } = IdToken?.Trim();
}

public class LoginWithGoogleValidator : AbstractValidator<LoginWithGoogleCommand>
{
    public LoginWithGoogleValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("ID Token is required.");
    }
}

public class LoginWithGoogleHandler(IAuthService authService) : IRequestHandler<LoginWithGoogleCommand>
{
    public async Task Handle(LoginWithGoogleCommand request, CancellationToken ct) =>
        await authService.SignInWithGoogleAsync(request.IdToken!, ct);
}
