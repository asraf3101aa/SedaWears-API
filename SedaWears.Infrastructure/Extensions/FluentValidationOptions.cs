using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SedaWears.Infrastructure.Extensions;

public class FluentValidationOptions<TOptions>(IServiceProvider serviceProvider, string? name) : IValidateOptions<TOptions> where TOptions : class
{
    public ValidateOptionsResult Validate(string? optionsName, TOptions options)
    {
        if (name != null && name != optionsName) return ValidateOptionsResult.Skip;
        ArgumentNullException.ThrowIfNull(options);

        // Resolve validator from a scope to handle any lifetime (Standard Practice)
        using var scope = serviceProvider.CreateScope();
        var validator = scope.ServiceProvider.GetService<IValidator<TOptions>>();

        // If no validator is registered, it's a configuration error since we expect a validator for all options
        if (validator == null) 
        {
            return ValidateOptionsResult.Fail($"No IValidator<{typeof(TOptions).Name}> found. All options registered with FluentValidation must have a corresponding validator.");
        }

        var result = validator.Validate(options);

        if (result.IsValid) return ValidateOptionsResult.Success;

        var errors = result.Errors.Select(e => $"Configuration Validation failed for {typeof(TOptions).Name}.{e.PropertyName}: {e.ErrorMessage}");
        return ValidateOptionsResult.Fail(errors);
    }
}
