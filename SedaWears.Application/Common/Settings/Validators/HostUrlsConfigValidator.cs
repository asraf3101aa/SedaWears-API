using FluentValidation;

namespace SedaWears.Application.Common.Settings.Validators;

public class HostUrlsConfigValidator : AbstractValidator<HostUrlsConfig>
{
    public HostUrlsConfigValidator()
    {
        RuleFor(x => x.Customer)
            .NotEmpty().WithMessage("The Customer Host URL is missing. Ensure 'HostUrls:Customer' is configured.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("The Customer Host URL must be a valid absolute URI.");

        RuleFor(x => x.Admin)
            .NotEmpty().WithMessage("The Admin Host URL is missing. Ensure 'HostUrls:Admin' is configured.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("The Admin Host URL must be a valid absolute URI.");

        RuleFor(x => x.Manager)
            .NotEmpty().WithMessage("The Manager Host URL is missing. Ensure 'HostUrls:Manager' is configured.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("The Manager Host URL must be a valid absolute URI.");

        RuleFor(x => x.Owner)
            .NotEmpty().WithMessage("The Owner Host URL is missing. Ensure 'HostUrls:Owner' is configured.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("The Owner Host URL must be a valid absolute URI.");
    }
}
