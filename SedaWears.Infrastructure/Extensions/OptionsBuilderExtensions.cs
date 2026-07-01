using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FluentValidation;


namespace SedaWears.Infrastructure.Extensions;

internal static class OptionsBuilderExtensions
{
    public static OptionsBuilder<TOptions> AddOptionsWithValidation<TOptions, TValidator>(
        this IServiceCollection services,
        string sectionName)
        where TOptions : class
        where TValidator : class, IValidator<TOptions>
    {
        return services.AddOptions<TOptions>()
            .BindConfiguration(sectionName)
            .ValidateFluentValidation<TOptions, TValidator>()
            .ValidateOnStart();
    }

    public static OptionsBuilder<TOptions> ValidateFluentValidation<TOptions, TValidator>(this OptionsBuilder<TOptions> optionsBuilder)
        where TOptions : class
        where TValidator : class, IValidator<TOptions>
    {
        optionsBuilder.Services.AddSingleton<IValidateOptions<TOptions>>(s =>
            new FluentValidationOptions<TOptions>(s, optionsBuilder.Name));
        return optionsBuilder;
    }

    public static IServiceCollection AddConfigWithValidation<TConfig, TValidator>(this IServiceCollection services, string sectionName)
        where TConfig : class
        where TValidator : class, IValidator<TConfig>
    {
        services.AddOptionsWithValidation<TConfig, TValidator>(sectionName);
        return services;
    }
}

