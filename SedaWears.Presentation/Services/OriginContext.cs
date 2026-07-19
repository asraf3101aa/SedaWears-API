using Microsoft.Extensions.Options;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Settings;
using SedaWears.Domain.Enums;

namespace SedaWears.Presentation.Services;

public class OriginContext(
    IHttpContextAccessor httpContextAccessor,
    IOptions<HostUrlsConfig> hostUrlsOptions) : IOriginContext
{
    public UserRole OriginRole
    {
        get
        {
            var context = httpContextAccessor.HttpContext;
            if (context == null) return UserRole.Customer;

            var origin = context.Request.Headers.Origin.ToString();

            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                return UserRole.Customer;

            var host = uri.Host;
            var hostUrls = hostUrlsOptions.Value;

            if (HostMatches(hostUrls.Admin, host)) return UserRole.Admin;
            if (HostMatches(hostUrls.Manager, host)) return UserRole.Manager;
            if (HostMatches(hostUrls.Owner, host)) return UserRole.Owner;

            return UserRole.Customer;
        }
    }

    private static bool HostMatches(string configuredUrl, string host)
    {
        return Uri.TryCreate(configuredUrl, UriKind.Absolute, out var configuredUri) &&
               string.Equals(configuredUri.Host, host, StringComparison.OrdinalIgnoreCase);
    }
}
