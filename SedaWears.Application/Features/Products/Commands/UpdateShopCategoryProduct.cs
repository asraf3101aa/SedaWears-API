using MediatR;
using FluentValidation;
using SedaWears.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Application.Common;
using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Application.Features.Products.Commands;

public record UpdateShopCategoryProductCommand(
    int ShopId,
    int CategoryId,
    int Id,
    string? Name,
    string? Description,
    decimal? Price,
    Gender? Gender,
    List<string>? ImageFileNames) : IRequest
{
    public string? Name { get; init; } = Name?.Trim();
    public string? Description { get; init; } = Description?.Trim();
}

public class UpdateShopCategoryProductValidator : AbstractValidator<UpdateShopCategoryProductCommand>
{
    public UpdateShopCategoryProductValidator()
    {
        RuleFor(x => x.ShopId)
            .GreaterThan(0).WithMessage("A valid shop identifier is required.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("A valid category identifier is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MinimumLength(10).WithMessage("Description must be at least 10 characters long.")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Price)
            .NotEmpty().WithMessage("Price is required.")
            .GreaterThan(0).WithMessage("Product price must be greater than zero.");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required.")
            .IsInEnum().WithMessage("A valid gender must be specified.");

    }
}

public class UpdateShopCategoryProductHandler(
    IApplicationDbContext dbContext,
    IFusionCache fusionCache,
    IUserService userService,
    ICurrentUser currentUser,
    IOriginContext originContext) : IRequestHandler<UpdateShopCategoryProductCommand>
{
    public async Task Handle(UpdateShopCategoryProductCommand request, CancellationToken ct)
    {
        var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();
        
        if (!user.Roles.Contains(originContext.OriginRole))
            throw new ForbiddenException("You are not authorized to update products.");

        var shopQuery = dbContext.Shops.AsNoTracking().Where(s => s.Id == request.ShopId);
        var userId = currentUser.Id;

        bool shopExists = originContext.OriginRole switch
        {
            UserRole.Admin => await shopQuery.AnyAsync(s => !s.IsDeleted, ct),
            UserRole.Owner => await shopQuery.AnyAsync(s => s.Id != 1 && !s.IsDeleted && s.Owners.Any(o => o.UserId == userId), ct),
            UserRole.Manager => await shopQuery.AnyAsync(s => s.Id != 1 && !s.IsDeleted && s.Managers.Any(m => m.UserId == userId), ct),
            _ => false
        };

        if (!shopExists)
            throw new ShopNotFoundException();

        var categoryExists = await dbContext.Categories
            .AnyAsync(c => c.Id == request.CategoryId && c.ShopId == request.ShopId && !c.IsDeleted, ct);
        if (!categoryExists)
        {
            throw new CategoryNotFoundException();
        }

        var productQuery = dbContext.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .Where(p => p.Id == request.Id && p.CategoryId == request.CategoryId && p.Category.ShopId == request.ShopId && !p.IsDeleted);

        var product = await productQuery.FirstOrDefaultAsync(ct) ?? throw new ProductNotFoundException();

        product.Name = request.Name!;
        product.Description = request.Description;
        product.Price = request.Price!.Value;
        product.CategoryId = request.CategoryId;
        product.Gender = request.Gender!.Value;

        if (request.ImageFileNames is { Count: > 0 })
        {
            product.Images.Clear();
            foreach (var (fileName, index) in request.ImageFileNames!.Select((v, i) => (v, i)))
            {
                product.Images.Add(new ProductImage { FileName = fileName, Order = index });
            }
        }

        await dbContext.SaveChangesAsync(ct);
        await fusionCache.RemoveAsync(CacheKeys.Product(request.Id), token: ct);
    }
}
