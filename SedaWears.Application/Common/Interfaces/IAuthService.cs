using SedaWears.Domain.Enums;

namespace SedaWears.Application.Common.Interfaces;

public interface IAuthService
{
    Task SignInAsync(string email, string password, bool rememberMe, UserRole role, CancellationToken ct = default);
    Task SignInWithGoogleAsync(string idToken, CancellationToken ct = default);
    Task SignOutAsync();
}
