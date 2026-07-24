using MediatR;
using FluentValidation;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Shops.Commands;

public record UpdateShopCommand(
    int Id,
    string? Name,
    string? SubdomainSlug,
    string? Description,
    string? LogoFileName = null,
    string? BannerFileName = null) : IRequest
{
    public string? Name { get; init; } = Name?.Trim();
    public string? SubdomainSlug { get; init; } = SubdomainSlug?.Trim();
    public string? Description { get; init; } = Description?.Trim();
    public string? LogoFileName { get; init; } = LogoFileName?.Trim();
    public string? BannerFileName { get; init; } = BannerFileName?.Trim();
}

public class UpdateShopValidator : AbstractValidator<UpdateShopCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateShopValidator(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Store name is required.")
            .MaximumLength(100).WithMessage("Store name must not exceed 100 characters.")
            .MustAsync(BeUniqueName).WithMessage("Store name already exists.");

        RuleFor(x => x.SubdomainSlug)
            .NotEmpty().WithMessage("Store slug is required.")
            .MaximumLength(100).WithMessage("Store slug must not exceed 100 characters.")
            .MustAsync(BeUniqueSubdomainSlug).WithMessage("Subdomain slug already exists.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters long.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.LogoFileName)
            .MaximumLength(255).WithMessage("Logo file name must not exceed 255 characters.");

        RuleFor(x => x.BannerFileName)
            .MaximumLength(255).WithMessage("Banner file name must not exceed 255 characters.");
    }

    private async Task<bool> BeUniqueName(UpdateShopCommand command, string name, CancellationToken ct)
    {
        return !await _dbContext.Shops.AnyAsync(x => !x.IsDeleted && x.Id != command.Id && EF.Functions.ILike(x.Name, name), ct);
    }

    private async Task<bool> BeUniqueSubdomainSlug(UpdateShopCommand command, string slug, CancellationToken ct)
    {
        return !await _dbContext.Shops.AnyAsync(x => !x.IsDeleted && x.Id != command.Id && EF.Functions.ILike(x.SubdomainSlug, slug), ct);
    }
}

public class UpdateShopHandler(IApplicationDbContext dbContext, ICurrentUser currentUser, IUserService userService) : IRequestHandler<UpdateShopCommand>
{
    public async Task Handle(UpdateShopCommand request, CancellationToken ct)
    {
        var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();
        
        if (!user.Roles.Any(r => r == UserRole.Admin || r == UserRole.Owner || r == UserRole.Manager))
            throw new ForbiddenException("You are not authorized to update this shop.");

        var shop = await dbContext.Shops.FirstOrDefaultAsync(s => s.Id == request.Id && !s.IsDeleted, ct)
            ?? throw new ShopNotFoundException();

        shop.Name = request.Name!;
        shop.SubdomainSlug = request.SubdomainSlug!;
        shop.Description = request.Description!;
        shop.LogoFileName = request.LogoFileName ?? shop.LogoFileName;
        shop.BannerFileName = request.BannerFileName ?? shop.BannerFileName;

        await dbContext.SaveChangesAsync(ct);
    }
}
