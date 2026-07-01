using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Settings;
using SedaWears.Infrastructure.Persistence;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace SedaWears.Infrastructure.Extensions;

internal static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var config = sp.GetRequiredService<IOptions<ConnectionStringsConfig>>().Value;
            options.UseNpgsql(config.Postgres, o =>
                o.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<ConnectionStringsConfig>>().Value;
            var options = ConfigurationOptions.Parse(config.Redis);
            return ConnectionMultiplexer.Connect(options);
        });

        services.AddDataProtection();
        services.AddOptions<KeyManagementOptions>()
            .Configure<IConnectionMultiplexer>((options, redis) =>
            {
                options.XmlRepository = new Microsoft.AspNetCore.DataProtection.StackExchangeRedis.RedisXmlRepository(() => redis.GetDatabase(), "DataProtection-Keys");
            });

        services.AddOptions<RedisCacheOptions>()
            .Configure<IConnectionMultiplexer>((options, multiplexer) =>
            {
                options.ConnectionMultiplexerFactory = () => Task.FromResult(multiplexer);
                options.InstanceName = "SedaWears_";
            });
        services.AddStackExchangeRedisCache(options => { });

        services.AddOptions<RedisBackplaneOptions>()
            .Configure<IConnectionMultiplexer>((options, multiplexer) =>
            {
                options.ConnectionMultiplexerFactory = () => Task.FromResult(multiplexer);
            });

        services.AddFusionCache()
            .WithSerializer(new FusionCacheSystemTextJsonSerializer())
            .WithRegisteredDistributedCache()
            .WithBackplane(sp => new RedisBackplane(
                sp.GetRequiredService<IOptions<RedisBackplaneOptions>>().Value));

        return services;
    }
}
