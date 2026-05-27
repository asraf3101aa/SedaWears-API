using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Presentation.Configurations;

public class CookieAuthenticationConfiguration(AppConfig appConfig) : IConfigureNamedOptions<CookieAuthenticationOptions>
{
    public void Configure(string? name, CookieAuthenticationOptions options)
    {
        if (name != IdentityConstants.ApplicationScheme) return;

        options.Cookie.Name = "SedaWears.Cookie";
        options.Cookie.Domain = appConfig.CookieDomain;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    }

    public void Configure(CookieAuthenticationOptions options) => Configure(IdentityConstants.ApplicationScheme, options);
}
