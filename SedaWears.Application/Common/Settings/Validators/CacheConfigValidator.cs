using FluentValidation;

namespace SedaWears.Application.Common.Settings.Validators;

public class CacheConfigValidator : AbstractValidator<CacheConfig>
{
    public CacheConfigValidator()
    {
        RuleFor(x => x.ProfileCacheDuration)
            .GreaterThan(TimeSpan.Zero).WithMessage("Profile cache duration must be greater than zero.");
            
        RuleFor(x => x.ProductCacheDuration)
            .GreaterThan(TimeSpan.Zero).WithMessage("Product cache duration must be greater than zero.");
            
        RuleFor(x => x.ProfileCacheEagerRefreshThreshold)
            .InclusiveBetween(0f, 1f).WithMessage("Profile cache eager refresh threshold must be between 0 and 1.");
            
        RuleFor(x => x.ProductCacheEagerRefreshThreshold)
            .InclusiveBetween(0f, 1f).WithMessage("Product cache eager refresh threshold must be between 0 and 1.");
    }
}
