using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Presentation.Extensions;

internal static class CorsExtensions
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors();

        services.AddOptions<CorsOptions>()
            .Configure<IOptions<CorsConfig>>((options, corsConfig) =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(corsConfig.Value.AllowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

        return services;
    }
}
