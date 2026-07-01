using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Infrastructure.Extensions;

internal static class MassTransitExtensions
{
    public static IServiceCollection AddMessageBus(this IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumers(typeof(DependencyInjection).Assembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                var config = context.GetRequiredService<IOptions<ConnectionStringsConfig>>().Value;
                cfg.Host(config.RabbitMQ);

                cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
