using MediatR;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common;
using SedaWears.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Application.Features.Categories.Commands;

public record DeleteShopCategoryCommand(int Id, int ShopId) : IRequest;

public class DeleteShopCategoryHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IUserService userService,
    IOriginContext originContext,
    IFusionCache fusionCache) : IRequestHandler<DeleteShopCategoryCommand>
{
    public async Task Handle(DeleteShopCategoryCommand request, CancellationToken ct)
    {
        var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();

        if (!user.Roles.Contains(originContext.OriginRole))
            throw new ForbiddenException("You are not authorized to delete categories for this shop.");

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

        var categoryQuery = dbContext.Categories.Where(c => c.Id == request.Id && c.ShopId == request.ShopId && !c.IsDeleted);

        var deletedRowsCount = await categoryQuery
            .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.IsDeleted, true), ct);

        if (deletedRowsCount == 0)
            throw new CategoryNotFoundException();

        await fusionCache.RemoveAsync(CacheKeys.Category(request.Id), token: ct);
    }
}
