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

public record GetProductQuery(int Id, int ShopId, int CategoryId) : IRequest<ProductDto>;

public class GetProductHandler(
    IApplicationDbContext dbContext,
    IUserService userService,
    ICurrentUser currentUser,
    IOriginContext originContext,
    IOptions<OpeninaryConfig> configOptions,
    IFusionCache fusionCache) : IRequestHandler<GetProductQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        if (request.ShopId == 1 && originContext.OriginRole is UserRole.Owner or UserRole.Manager)
            throw new ProductNotFoundException();

        if (originContext.OriginRole == UserRole.Customer)
        {
            return await fusionCache.GetOrSetAsync(
                CacheKeys.Product(request.Id),
                async (ct) =>
                {
                    return await dbContext.Products
                        .AsNoTracking()
                        .Where(p => p.Id == request.Id
                            && p.CategoryId == request.CategoryId
                            && p.Category.ShopId == request.ShopId
                            && p.IsActive && p.Category.IsActive && p.Category.Shop.IsActive)
                        .ProjectToProduct(configOptions.Value.BaseUrl)
                        .FirstOrDefaultAsync(ct) ?? throw new ProductNotFoundException();
                },
                CachePolicies.Product,
                token: cancellationToken);
        }

        var user = await userService.FindByIdAsync(currentUser.Id, cancellationToken) ?? throw new UnauthorizedAccessException();

        if (!user.Roles.Contains(originContext.OriginRole))
            throw new ForbiddenException("You are not authorized to view this product.");

        var userId = currentUser.Id;
        var query = dbContext.Products
            .Where(p => p.Id == request.Id && p.CategoryId == request.CategoryId && p.Category.ShopId == request.ShopId);

        query = originContext.OriginRole switch
        {
            UserRole.Admin => query,
            UserRole.Owner => query.Where(p => p.Category.Shop.Owners.Any(o => o.UserId == userId)),
            UserRole.Manager => query.Where(p => p.Category.Shop.Managers.Any(m => m.UserId == userId)),
            _ => throw new ForbiddenException("You are not authorized to view this product.")
        };

        return await query
            .AsNoTracking()
            .ProjectToProduct(configOptions.Value.BaseUrl)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new ProductNotFoundException();
    }
}
