using MediatR;
using SedaWears.Application.Common.Interfaces;

namespace SedaWears.Application.Features.Auth.Commands;

public record LogoutCommand : IRequest;

public class LogoutHandler(IAuthService authService) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken ct)
    {
        await authService.SignOutAsync();
    }
}
