using FluentValidation;

namespace SedaWears.Application.Common.Settings.Validators;

public class AuthConfigValidator : AbstractValidator<AuthConfig>
{
    public AuthConfigValidator()
    {
        RuleFor(x => x.CookieDomain)
            .NotEmpty().WithMessage("The Cookie Domain is missing. Ensure 'Auth:CookieDomain' is configured.")
            .Must(domain => domain.StartsWith('.') && domain.Length > 1 && !domain.Contains('/') && !domain.Contains(':'))
            .WithMessage("Cookie domain must start with a dot (e.g., '.sedawears.com') and not contain protocols, ports, or paths.");

        RuleFor(x => x.AdminInvitationExpiry)
            .GreaterThan(0).WithMessage("AdminInvitationExpiry must be greater than 0 hours.");

        RuleFor(x => x.OwnerInvitationExpiry)
            .GreaterThan(0).WithMessage("OwnerInvitationExpiry must be greater than 0 hours.");

        RuleFor(x => x.ManagerInvitationExpiry)
            .GreaterThan(0).WithMessage("ManagerInvitationExpiry must be greater than 0 hours.");
    }
}
