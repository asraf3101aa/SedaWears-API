using FluentValidation;

namespace SedaWears.Application.Common.Settings.Validators;

public class ConnectionStringsConfigValidator : AbstractValidator<ConnectionStringsConfig>
{
    public ConnectionStringsConfigValidator()
    {
        RuleFor(x => x.Postgres)
            .NotEmpty().WithMessage("The database connection string is missing. Ensure 'ConnectionStrings:Postgres' is configured.");

        RuleFor(x => x.RedisCertThumbprint)
            .Matches(@"^[0-9a-fA-F]{64}$")
            .When(x => !string.IsNullOrEmpty(x.RedisCertThumbprint))
            .WithMessage("The Redis certificate thumbprint must be a valid 64-character SHA-256 hexadecimal string.");
    }
}
