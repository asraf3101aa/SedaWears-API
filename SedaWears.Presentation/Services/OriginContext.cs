using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Enums;

namespace SedaWears.Presentation.Services;

public class OriginContext(
    IHttpContextAccessor httpContextAccessor) : IOriginContext
{
    public UserRole CurrentRole
    {
        get
        {
            var context = httpContextAccessor.HttpContext;
            if (context == null) return UserRole.Customer;

            var origin = context.Request.Headers["Origin"].ToString();

            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                return UserRole.Customer;

            var host = uri.Host.ToLower();

            if (host.StartsWith("admin.")) return UserRole.Admin;
            if (host.StartsWith("manager.")) return UserRole.Manager;
            if (host.StartsWith("owner.")) return UserRole.Owner;

            return UserRole.Customer;
        }
    }
}
