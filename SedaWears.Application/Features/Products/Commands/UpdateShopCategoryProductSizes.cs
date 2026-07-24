using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;
using SedaWears.Application.Features.Products.Models;
using SedaWears.Application.Common;
using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Application.Features.Products.Commands;

public record UpdateShopCategoryProductSizesCommand(int ShopId, int CategoryId, int Id, List<ProductSizeDto>? Sizes) : IRequest;

public class UpdateShopCategoryProductSizesValidator : AbstractValidator<UpdateShopCategoryProductSizesCommand>
{
    public UpdateShopCategoryProductSizesValidator()
    {

        RuleFor(x => x.Sizes)
            .NotEmpty().WithMessage("At least one size with stock must be provided.");
    }
}

public class UpdateShopCategoryProductSizesHandler(
    IApplicationDbContext dbContext,
    IFusionCache fusionCache,
    IUserService userService,
    ICurrentUser currentUser,
    IOriginContext originContext) : IRequestHandler<UpdateShopCategoryProductSizesCommand>
{
    public async Task Handle(UpdateShopCategoryProductSizesCommand request, CancellationToken ct)
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
            .Include(p => p.SizeStocks)
            .Include(p => p.Category)
            .Where(p => p.Id == request.Id && p.CategoryId == request.CategoryId && p.Category.ShopId == request.ShopId && !p.IsDeleted);

        var product = await productQuery.FirstOrDefaultAsync(ct) ?? throw new ProductNotFoundException();

        // Sync sizes
        product.SizeStocks.Clear();
        foreach (var s in request.Sizes!)
        {
            product.SizeStocks.Add(new ProductSizeStock { Size = s.Size, Stock = s.Stock });
        }

        await dbContext.SaveChangesAsync(ct);
        await fusionCache.RemoveAsync(CacheKeys.Product(product.Id), token: ct);
    }
}
