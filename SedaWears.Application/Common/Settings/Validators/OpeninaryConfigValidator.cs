using FluentValidation;

namespace SedaWears.Application.Common.Settings.Validators;

public class OpeninaryConfigValidator : AbstractValidator<OpeninaryConfig>
{
    public OpeninaryConfigValidator()
    {
        RuleFor(x => x.BaseUrl)
            .NotEmpty().WithMessage("The Openinary Base Url is missing. Ensure 'Openinary:BaseUrl' is set.");
    }
}
