using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HostFiltering;
using SedaWears.Presentation.Configurations;

using SedaWears.Application;
using SedaWears.Infrastructure;
using SedaWears.Application.Common.Settings;
using SedaWears.Infrastructure.RateLimiting;
using System.Text.Json;
using System.Text.Json.Serialization;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Presentation.Services;
using SedaWears.Presentation.Middleware;

using SedaWears.Application.Common;
using System.Net;

namespace SedaWears.Presentation.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IOriginContext, OriginContext>();
        services.AddApplication();
        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.ConfigureOptions<CookieAuthenticationConfiguration>();

        return services;
    }


    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RateLimitingConfig>(configuration.GetSection("RateLimiting"));
        services.AddCustomRateLimiting();
        return services;
    }

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                                     | ForwardedHeaders.XForwardedProto
                                     | ForwardedHeaders.XForwardedHost;
        });

        services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Disable built-in validation so that FluentValidation in MediatR is the single source of truth
        options.SuppressModelStateInvalidFilter = true;
    });

        services.AddCors();
        services.ConfigureOptions<CorsConfiguration>();
        services.ConfigureOptions<HostFilteringConfiguration>();

        return services;
    }
}
