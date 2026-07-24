using MediatR;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Application.Features.Categories.Commands;

public record UpdateShopCategoryActiveStatusCommand(int Id, bool? IsActive, int? ShopId = null) : IRequest;

public class UpdateShopCategoryActiveStatusHandler(
    IApplicationDbContext dbContext,
    IUserService userService,
    ICurrentUser currentUser,
    IOriginContext originContext,
    IFusionCache fusionCache) : IRequestHandler<UpdateShopCategoryActiveStatusCommand>
{
    public const int MaxActiveCategoriesPerShop = 7;

    public async Task Handle(UpdateShopCategoryActiveStatusCommand request, CancellationToken ct)
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
            throw new ForbiddenException("Only administrators can update global categories.");
        }

        var categoryQuery = dbContext.Categories.Where(c => c.Id == request.Id && c.ShopId == request.ShopId && !c.IsDeleted);

        var categoryExists = await categoryQuery.AnyAsync(ct);
        if (!categoryExists)
            throw new CategoryNotFoundException();

        if (request.IsActive!.Value)
        {
            var activeCount = await dbContext.Categories
                .CountAsync(c => c.ShopId == request.ShopId && c.IsActive && !c.IsDeleted, ct);

            if (activeCount >= MaxActiveCategoriesPerShop)
                throw new ConflictException($"A shop cannot have more than {MaxActiveCategoriesPerShop} active categories.");
        }

        var updated = await categoryQuery.ExecuteUpdateAsync(s => s.SetProperty(c => c.IsActive, request.IsActive!.Value), ct);
        if (updated > 0)
            await fusionCache.RemoveAsync(CacheKeys.Category(request.Id), token: ct);
    }
}
