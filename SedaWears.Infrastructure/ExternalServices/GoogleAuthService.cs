using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Infrastructure.ExternalServices;

public class GoogleAuthService(IOptions<GoogleConfig> googleConfig) : IGoogleAuthService
{
    public async Task<GoogleUser?> ValidateTokenAsync(string idToken)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [googleConfig.Value.ClientId]
            });

            return new GoogleUser(payload.Email, payload.GivenName, payload.FamilyName);
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }
}
