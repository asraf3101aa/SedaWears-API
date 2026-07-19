using System.Reflection;
using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SedaWears.Application.Common.Behaviors;
using SedaWears.Application.Common.Interfaces;


namespace SedaWears.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {

        var assembly = Assembly.GetExecutingAssembly();

        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue;
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
        ValidatorOptions.Global.PropertyNameResolver = (_, member, _) => member != null ? JsonNamingPolicy.CamelCase.ConvertName(member.Name) : null;

        services.AddValidatorsFromAssembly(assembly);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return services;
    }
}
