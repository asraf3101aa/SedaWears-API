using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Common.Settings.Validators;
using SedaWears.Infrastructure.Persistence;
using SedaWears.Infrastructure.Services;
using SedaWears.Infrastructure.ExternalServices;
using SedaWears.Infrastructure.Configurations;
using SedaWears.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FluentValidation;
using Resend;
using StackExchange.Redis;

namespace SedaWears.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddInfrastructureConfigs()
            .AddPersistence(configuration)
            .AddIdentity()
            .AddResendEmail()
            .AddInfrastructureServices();

        return services;
    }

    private static IServiceCollection AddInfrastructureConfigs(this IServiceCollection services)
    {
        // 1. Scan for all validators once
        services.AddValidatorsFromAssemblyContaining<RateLimitingConfigValidator>();

        // 2. Register Configurations using BindConfiguration (Modern .NET idiomatic way)
        // This also registers the direct type (e.g. JwtConfig) as a Singleton for easier DI.
        services
            .AddConfigWithValidation<ConnectionStringsConfig, ConnectionStringsConfigValidator>("ConnectionStrings")
            .AddConfigWithValidation<GoogleConfig, GoogleConfigValidator>("Google")
            .AddConfigWithValidation<AppConfig, AppConfigValidator>("App")
            .AddConfigWithValidation<S3Config, S3ConfigValidator>("S3")
            .AddConfigWithValidation<OpeninaryConfig, OpeninaryConfigValidator>("Openinary")
            .AddConfigWithValidation<EmailConfig, EmailConfigValidator>("Email")
            .AddConfigWithValidation<ResendConfig, ResendConfigValidator>("Resend")
            .AddConfigWithValidation<RateLimitingConfig, RateLimitingConfigValidator>("RateLimiting");

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var config = sp.GetRequiredService<ConnectionStringsConfig>();
            options.UseNpgsql(config.Postgres, o =>
                o.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var config = sp.GetRequiredService<ConnectionStringsConfig>();
            return ConnectionMultiplexer.Connect(config.Redis);
        });

        services.AddDataProtection();
        services.AddOptions<KeyManagementOptions>()
            .Configure<IConnectionMultiplexer>((options, multiplexer) =>
            {
                options.XmlRepository = new RedisXmlRepository(() => multiplexer.GetDatabase(), "DataProtection-Keys");
            });

        return services;
    }

    private static IServiceCollection AddIdentity(this IServiceCollection services)
    {
        services.AddIdentity<User, IdentityRole<int>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    private static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IS3Service, S3Service>();

        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }

    private static IServiceCollection AddResendEmail(this IServiceCollection services)
    {
        services.AddOptions();
        services.AddHttpClient<ResendClient>();
        services.AddOptions<ResendClientOptions>()
            .Configure<ResendConfig>((options, config) =>
            {
                options.ApiToken = config.ApiKey;
            });
        services.AddTransient<IResend, ResendClient>();

        return services;
    }

    /// <summary>
    /// Helper to register both IOptions<T> (idiomatic .NET pattern) and the raw T config class (easier injection).
    /// Uses modern .NET BindConfiguration to resolve settings from the container's IConfiguration.
    /// </summary>
    private static IServiceCollection AddConfigWithValidation<TConfig, TValidator>(this IServiceCollection services, string sectionName)
        where TConfig : class
        where TValidator : class, IValidator<TConfig>
    {
        services.AddOptionsWithValidation<TConfig, TValidator>(sectionName);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<TConfig>>().Value);
        return services;
    }
}
