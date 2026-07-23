using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common;
using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Application.Features.Products.Commands;

public record DeleteProductCommand(int ShopId, int CategoryId, int Id) : IRequest<Unit>;

public class DeleteProductHandler(IApplicationDbContext dbContext, IFusionCache fusionCache) : IRequestHandler<DeleteProductCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await dbContext.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct) ?? throw new ProductNotFoundException();

        if (product.Category.ShopId != request.ShopId || product.CategoryId != request.CategoryId)
        {
            throw new ProductNotFoundException();
        }

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(ct);
        await fusionCache.RemoveAsync(CacheKeys.Product(request.Id), token: ct);
        return Unit.Value;
    }
}
