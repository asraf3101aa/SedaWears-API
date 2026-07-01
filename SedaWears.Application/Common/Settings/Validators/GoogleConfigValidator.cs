using FluentValidation;

namespace SedaWears.Application.Common.Settings.Validators;

public class GoogleConfigValidator : AbstractValidator<GoogleConfig>
{
    public GoogleConfigValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("The Google Client ID is missing. Ensure 'Google:ClientId' is set for social login.");
    }
}
