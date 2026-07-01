using FluentValidation;

namespace SedaWears.Application.Common.Settings.Validators;

public class CorsConfigValidator : AbstractValidator<CorsConfig>
{
    public CorsConfigValidator()
    {
        RuleFor(x => x.AllowedOrigins)
            .NotEmpty().WithMessage("CORS Allowed Origins are missing. Ensure 'Cors:AllowedOrigins' is configured.")
            .Must(origins =>
            {
                if (origins == null || origins.Length == 0) return false;
                foreach (var part in origins)
                {
                    if (string.IsNullOrWhiteSpace(part) || !Uri.TryCreate(part, UriKind.Absolute, out _))
                        return false;
                }
                return true;
            })
            .WithMessage("All CORS Allowed Origins must be valid absolute URIs (e.g. 'http://localhost:3000').");
    }
}
