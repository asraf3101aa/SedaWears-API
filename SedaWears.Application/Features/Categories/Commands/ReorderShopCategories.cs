using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common;
using SedaWears.Domain.Enums;
using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Application.Features.Categories.Commands;

public record ReorderCategoryItem(int Id, int DisplayOrder);

public record ReorderShopCategoriesCommand(List<ReorderCategoryItem>? Orders, int? ShopId = null) : IRequest;

public class ReorderShopCategoriesHandler(
    IApplicationDbContext dbContext,
    IUserService userService,
    ICurrentUser currentUser,
    IOriginContext originContext,
    IFusionCache fusionCache) : IRequestHandler<ReorderShopCategoriesCommand>
{
    public async Task Handle(ReorderShopCategoriesCommand request, CancellationToken ct)
    {
        var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();
        
        if (!user.Roles.Contains(originContext.OriginRole))
            throw new ForbiddenException("You are not authorized to update categories.");

        if (request.ShopId.HasValue)
        {
            var shopQuery = dbContext.Shops.AsNoTracking().Where(s => s.Id == request.ShopId.Value);
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
        }
        else if (originContext.OriginRole != UserRole.Admin)
        {
            throw new ForbiddenException("Only administrators can reorder global categories.");
        }

        var orderItemIds = request.Orders!.Select(o => o.Id).ToList();
        var query = dbContext.Categories.Where(c => orderItemIds.Contains(c.Id) && !c.IsDeleted);
        if (request.ShopId.HasValue) query = query.Where(c => c.ShopId == request.ShopId);

        var categories = await query.ToListAsync(ct);

        foreach (var order in request.Orders!)
        {
            var category = categories.FirstOrDefault(c => c.Id == order.Id);
            if (category != null) category.DisplayOrder = order.DisplayOrder;
        }

        await dbContext.SaveChangesAsync(ct);

        foreach (var order in request.Orders!)
        {
            await fusionCache.RemoveAsync(CacheKeys.Category(order.Id), token: ct);
        }
    }
}
