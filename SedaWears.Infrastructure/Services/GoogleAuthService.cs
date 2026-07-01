using Google.Apis.Auth;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Settings;

using Microsoft.Extensions.Options;

namespace SedaWears.Infrastructure.Services;

public class GoogleAuthService(IOptions<GoogleConfig> googleConfigOptions) : IGoogleAuthService
{
    private readonly GoogleConfig _googleConfig = googleConfigOptions.Value;

    public async Task<GoogleUser?> ValidateTokenAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings { Audience = [_googleConfig.ClientId] };
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            if (payload == null) return null;
            return new GoogleUser(payload.Email, payload.GivenName ?? "", payload.FamilyName ?? "");
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }
}
