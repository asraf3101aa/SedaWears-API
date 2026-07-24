using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common;
using ZiggyCreatures.Caching.Fusion;

using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Products.Commands;

public record DeleteShopCategoryProductCommand(int ShopId, int CategoryId, int Id) : IRequest;

public class DeleteShopCategoryProductHandler(
    IApplicationDbContext dbContext,
    IFusionCache fusionCache,
    IUserService userService,
    ICurrentUser currentUser,
    IOriginContext originContext) : IRequestHandler<DeleteShopCategoryProductCommand>
{
    public async Task Handle(DeleteShopCategoryProductCommand request, CancellationToken ct)
    {
        var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();
        
        if (!user.Roles.Contains(originContext.OriginRole))
            throw new ForbiddenException("You are not authorized to delete products.");

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
            .Where(p => p.Id == request.Id && p.CategoryId == request.CategoryId && p.Category.ShopId == request.ShopId && !p.IsDeleted);

        var productExists = await productQuery.AnyAsync(ct);
        if (!productExists)
            throw new ProductNotFoundException();

        var updated = await productQuery.ExecuteUpdateAsync(s => s
            .SetProperty(p => p.IsDeleted, true)
            .SetProperty(p => p.IsActive, false), ct);

        if (updated > 0) await fusionCache.RemoveAsync(CacheKeys.Product(request.Id), token: ct);
    }
}
