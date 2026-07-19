using SedaWears.Presentation.Middleware;
using SedaWears.Presentation.Extensions;

namespace SedaWears.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddContexts();

        services.AddHealthChecks();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddForwardedHeaders();
        services.AddApiControllers();

        services.AddCorsPolicy();
        services.AddCookieAuthentication();

        return services;
    }
}
