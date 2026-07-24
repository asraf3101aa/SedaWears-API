using Microsoft.Extensions.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Features.Products.Models;
using SedaWears.Application.Features.Products.Projections;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Common;
using SedaWears.Domain.Enums;
using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Application.Features.Products.Queries;

public record GetShopCategoryProductQuery(int Id, int ShopId, int CategoryId) : IRequest<ProductDto>;

public class GetShopCategoryProductHandler(
    IApplicationDbContext dbContext,
    IUserService userService,
    ICurrentUser currentUser,
    IOriginContext originContext,
    IOptions<OpeninaryConfig> configOptions,
    IFusionCache fusionCache) : IRequestHandler<GetShopCategoryProductQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetShopCategoryProductQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == request.Id && p.CategoryId == request.CategoryId && p.Category.ShopId == request.ShopId);



        if (originContext.OriginRole == UserRole.Customer)
        {
            return await fusionCache.GetOrSetAsync(
                CacheKeys.Product(request.Id),
                async (ct) =>
                {
                    return await query
                        .Where(p => p.IsActive && !p.IsDeleted 
                            && p.Category.IsActive && !p.Category.IsDeleted 
                            && p.Category.Shop.IsActive && !p.Category.Shop.IsDeleted)
                        .ProjectToProduct(configOptions.Value.BaseUrl)
                        .FirstOrDefaultAsync(ct) ?? throw new ProductNotFoundException();
                },
                CachePolicies.Product,
                token: cancellationToken);
        }

        var shopQuery = dbContext.Shops.AsNoTracking().Where(s => s.Id == request.ShopId);

        var user = await userService.FindByIdAsync(currentUser.Id, cancellationToken) ?? throw new UnauthorizedAccessException();

        if (!user.Roles.Contains(originContext.OriginRole))
            throw new ForbiddenException("You are not authorized to view this product.");

        var userId = currentUser.Id;
        bool shopExists = originContext.OriginRole switch
        {
            UserRole.Admin => await shopQuery.AnyAsync(s => !s.IsDeleted, cancellationToken),
            UserRole.Owner => await shopQuery.AnyAsync(s => s.Id != 1 && !s.IsDeleted && s.Owners.Any(o => o.UserId == userId), cancellationToken),
            UserRole.Manager => await shopQuery.AnyAsync(s => s.Id != 1 && !s.IsDeleted && s.Managers.Any(m => m.UserId == userId), cancellationToken),
            _ => false
        };

        if (!shopExists)
            throw new ShopNotFoundException();

        bool categoryExists = await dbContext.Categories
            .AnyAsync(c => c.Id == request.CategoryId && c.ShopId == request.ShopId && !c.IsDeleted, cancellationToken);

        if (!categoryExists)
            throw new CategoryNotFoundException();



        if (originContext.OriginRole != UserRole.Admin)
        {
            query = query.Where(p => !p.IsDeleted);
        }

        return await query
            .ProjectToProduct(configOptions.Value.BaseUrl)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new ProductNotFoundException();
    }
}
