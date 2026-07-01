using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common;
using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Application.Features.Products.Commands;

public record DeleteProductCommand(int Id, int? ShopId = null) : IRequest<Unit>;

public class DeleteProductHandler(IApplicationDbContext dbContext, IFusionCache fusionCache) : IRequestHandler<DeleteProductCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await dbContext.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct) ?? throw new ProductNotFoundException();

        if (request.ShopId.HasValue && product.Category.ShopId != request.ShopId)
        {
            throw new ProductNotFoundException();
        }

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(ct);
        await fusionCache.RemoveAsync(CacheKeys.Product(request.Id), token: ct);
        return Unit.Value;
    }
}
