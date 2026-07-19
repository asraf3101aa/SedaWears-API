using System.Security.Claims;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Enums;

namespace SedaWears.Presentation.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public int Id
    {
        get
        {
            var sub = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(sub, out var id) ? id : throw new UnauthorizedAccessException();
        }
    }

    public int? ShopId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null) return null;

            if (user.IsInRole(nameof(UserRole.Owner)) || user.IsInRole(nameof(UserRole.Manager)))
            {
                var headerValue = httpContextAccessor.HttpContext?.Request.Headers["X-Shop-ID"].ToString();
                return int.TryParse(headerValue, out var shopId) ? shopId : null;
            }

            return null;
        }
    }
}
