using SedaWears.Application.Common.Settings;
using SedaWears.Application.Common.Settings.Validators;
using SedaWears.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace SedaWears.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services
            .AddInfrastructureConfigs()
            .AddPersistence()
            .AddIdentity()
            .AddInfrastructureServices()
            .AddResendEmail()
            .AddS3()
            .AddMessageBus();

        return services;
    }

    private static IServiceCollection AddInfrastructureConfigs(this IServiceCollection services)
    {
        services
            .AddConfigWithValidation<ConnectionStringsConfig, ConnectionStringsConfigValidator>("ConnectionStrings")
            .AddConfigWithValidation<GoogleConfig, GoogleConfigValidator>("Google")
            .AddConfigWithValidation<CorsConfig, CorsConfigValidator>("Cors")
            .AddConfigWithValidation<AuthConfig, AuthConfigValidator>("Auth")
            .AddConfigWithValidation<HostUrlsConfig, HostUrlsConfigValidator>("HostUrls")
            .AddConfigWithValidation<S3Config, S3ConfigValidator>("S3")
            .AddConfigWithValidation<OpeninaryConfig, OpeninaryConfigValidator>("Openinary")
            .AddConfigWithValidation<EmailConfig, EmailConfigValidator>("Email")
            .AddConfigWithValidation<ResendConfig, ResendConfigValidator>("Resend");

        return services;
    }


}
