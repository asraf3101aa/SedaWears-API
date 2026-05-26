using FluentValidation;

namespace SedaWears.Application.Common.Settings.Validators;

public class AppConfigValidator : AbstractValidator<AppConfig>
{
    public AppConfigValidator()
    {
        RuleFor(x => x.CustomerFrontendUrl)
            .NotEmpty().WithMessage("The Customer Frontend URL is missing. Ensure 'AppConfig:CustomerFrontendUrl' is configured.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("The Customer Frontend URL must be a valid absolute URI.");

        RuleFor(x => x.AdminFrontendUrl)
            .NotEmpty().WithMessage("The Admin Frontend URL is missing. Ensure 'AppConfig:AdminFrontendUrl' is configured.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("The Admin Frontend URL must be a valid absolute URI.");

        RuleFor(x => x.ManagerFrontendUrl)
            .NotEmpty().WithMessage("The Manager Frontend URL is missing. Ensure 'AppConfig:ManagerFrontendUrl' is configured.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("The Manager Frontend URL must be a valid absolute URI.");

        RuleFor(x => x.OwnerFrontendUrl)
            .NotEmpty().WithMessage("The Owner Frontend URL is missing. Ensure 'AppConfig:OwnerFrontendUrl' is configured.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("The Owner Frontend URL must be a valid absolute URI.");

        RuleFor(x => x.CookieDomain)
            .NotEmpty().WithMessage("The Cookie Domain is missing. Ensure 'AppConfig:CookieDomain' is configured.")
            .Must(domain => domain.StartsWith('.') && domain.Length > 1 && !domain.Contains('/') && !domain.Contains(':'))
            .WithMessage("Cookie domain must start with a dot (e.g., '.sedawears.com') and not contain protocols, ports, or paths.");

        RuleFor(x => x.Cors)
            .NotNull().WithMessage("CORS configuration is missing.");

        RuleFor(x => x.Cors.AllowedOrigins)
            .NotEmpty().WithMessage("CORS Allowed Origins are missing. Ensure 'AppConfig:Cors:AllowedOrigins' is configured.")
            .Must(origins =>
            {
                if (string.IsNullOrWhiteSpace(origins)) return false;
                var parts = origins.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length == 0) return false;
                foreach (var part in parts)
                {
                    if (!Uri.TryCreate(part, UriKind.Absolute, out _))
                        return false;
                }
                return true;
            })
            .WithMessage("All CORS Allowed Origins must be valid absolute URIs separated by semicolons (e.g. 'http://localhost:3000;https://sedawears.com').");

        RuleFor(x => x.AdminInvitationExpiry)
            .GreaterThan(0).WithMessage("AdminInvitationExpiry must be greater than 0 hours.");

        RuleFor(x => x.OwnerInvitationExpiry)
            .GreaterThan(0).WithMessage("OwnerInvitationExpiry must be greater than 0 hours.");

        RuleFor(x => x.ManagerInvitationExpiry)
            .GreaterThan(0).WithMessage("ManagerInvitationExpiry must be greater than 0 hours.");
    }
}
