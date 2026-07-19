using MediatR;
using FluentValidation;
using SedaWears.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using SedaWears.Domain.Entities;

namespace SedaWears.Application.Features.Shops.Commands;

public record CreateShopCommand(
    string? Name,
    string? SubdomainSlug,
    string? Description,
    string? LogoFileName = null,
    string? BannerFileName = null) : IRequest;

public class CreateShopValidator : AbstractValidator<CreateShopCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateShopValidator(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Shop name is required.")
            .MaximumLength(50).WithMessage("Shop name must not exceed 100 characters.")
            .MustAsync(BeUniqueName).WithMessage("Shop name already exists.");

        RuleFor(x => x.SubdomainSlug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(50).WithMessage("Slug must not exceed 100 characters.")
            .MustAsync(BeUniqueSubdomainSlug).WithMessage("Subdomain slug already exists.");

        RuleFor(x => x.Description)
            .MinimumLength(10).WithMessage("Description must be at least 10 characters long.")
            .MaximumLength(250).WithMessage("Description must not exceed 300 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.LogoFileName)
            .MaximumLength(255).WithMessage("Logo file name must not exceed 255 characters.");

        RuleFor(x => x.BannerFileName)
            .MaximumLength(255).WithMessage("Banner file name must not exceed 255 characters.");
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken ct)
    {
        return !await _dbContext.Shops.AnyAsync(x => EF.Functions.ILike(x.Name, name), ct);
    }

    private async Task<bool> BeUniqueSubdomainSlug(string slug, CancellationToken ct)
    {
        return !await _dbContext.Shops.AnyAsync(x => EF.Functions.ILike(x.SubdomainSlug, slug), ct);
    }
}

public class CreateShopHandler(IApplicationDbContext dbContext) : IRequestHandler<CreateShopCommand>
{
    public async Task Handle(CreateShopCommand request, CancellationToken ct)
    {
        var shop = new Shop
        {
            Name = request.Name!,
            SubdomainSlug = request.SubdomainSlug!,
            Description = request.Description,
            LogoFileName = request.LogoFileName,
            BannerFileName = request.BannerFileName
        };

        dbContext.Shops.Add(shop);
        await dbContext.SaveChangesAsync(ct);
    }
}
