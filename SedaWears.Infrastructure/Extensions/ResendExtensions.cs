using Microsoft.Extensions.DependencyInjection;
using Resend;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Infrastructure.Extensions;

internal static class ResendExtensions
{
    public static IServiceCollection AddResendEmail(this IServiceCollection services)
    {
        services.AddHttpClient<IResend, ResendClient>();

        services.AddOptions<ResendClientOptions>()
            .Configure<ResendConfig>((options, config) =>
            {
                options.ApiToken = config.ApiKey;
            });

        return services;
    }
}
