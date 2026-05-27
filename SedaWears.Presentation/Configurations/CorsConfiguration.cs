using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Presentation.Configurations;

public class CorsConfiguration(AppConfig appConfig) : IConfigureOptions<CorsOptions>
{
    public void Configure(CorsOptions options)
    {
        var origins = (appConfig.Cors.AllowedOrigins ?? string.Empty)
            .Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        options.AddPolicy("Default", policy =>
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials());
    }
}
