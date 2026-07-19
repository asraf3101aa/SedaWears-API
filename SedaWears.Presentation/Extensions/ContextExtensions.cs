using SedaWears.Application.Common.Interfaces;
using SedaWears.Presentation.Services;

namespace SedaWears.Presentation.Extensions;

internal static class ContextExtensions
{
    public static IServiceCollection AddContexts(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IOriginContext, OriginContext>();

        return services;
    }
}
