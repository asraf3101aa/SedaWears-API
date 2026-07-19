using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;
using SedaWears.Application.Common;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Entities;

namespace SedaWears.Infrastructure.Services;

public class ProductService(IApplicationDbContext dbContext, IFusionCache fusionCache) : IProductService
{
    public async Task<Product> GetProductByIdAsync(int productId, CancellationToken ct = default)
    {
        return await fusionCache.GetOrSetAsync<Product>(
            CacheKeys.Product(productId),
            async (_, innerCt) =>
            {
                var product = await dbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId, innerCt);
                if (product == null) throw new ProductNotFoundException();
                return product;
            },
            token: ct) ?? throw new ProductNotFoundException();
    }
}
