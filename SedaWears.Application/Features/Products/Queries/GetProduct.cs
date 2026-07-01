using Microsoft.Extensions.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Features.Products.Models;
using SedaWears.Application.Features.Products.Projections;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Common;
using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Application.Features.Products.Queries;

public record GetProductQuery(int Id) : IRequest<ProductDto>;

public class GetProductHandler(IApplicationDbContext dbContext, IOptions<OpeninaryConfig> configOptions, IOptions<CacheConfig> cacheConfigOptions, IFusionCache fusionCache) : IRequestHandler<GetProductQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductQuery request, CancellationToken ct)
    {
        return await fusionCache.GetOrSetAsync<ProductDto>(
            CacheKeys.Product(request.Id),
            async (ctx, token) =>
            {
                return await dbContext.Products
                    .AsNoTracking()
                    .Where(p => p.Id == request.Id)
                    .ProjectToProduct(configOptions.Value.BaseUrl)
                    .FirstOrDefaultAsync(token) ?? throw new ProductNotFoundException();
            },
            new FusionCacheEntryOptions
            {
                Duration = cacheConfigOptions.Value.ProductCacheDuration
            },
            token: ct);
    }
}
