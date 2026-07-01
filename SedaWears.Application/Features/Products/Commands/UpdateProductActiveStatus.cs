using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;

using SedaWears.Application.Common;
using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Application.Features.Products.Commands;

using FluentValidation;

public record UpdateProductActiveStatusCommand(int Id, bool? IsActive, int? ShopId = null) : IRequest;

public class UpdateProductActiveStatusValidator : AbstractValidator<UpdateProductActiveStatusCommand>
{
    public UpdateProductActiveStatusValidator()
    {
        RuleFor(x => x.IsActive)
            .NotNull().WithMessage("IsActive status is required.");
    }
}

public class UpdateProductActiveStatusHandler(IApplicationDbContext dbContext, IFusionCache fusionCache) : IRequestHandler<UpdateProductActiveStatusCommand>
{
    public async Task Handle(UpdateProductActiveStatusCommand request, CancellationToken ct)
    {
        var product = await dbContext.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct) ?? throw new ProductNotFoundException();

        if (request.ShopId.HasValue && product.Category.ShopId != request.ShopId)
        {
            throw new ProductNotFoundException();
        }

        product.IsActive = request.IsActive!.Value;
        await dbContext.SaveChangesAsync(ct);
        await fusionCache.RemoveAsync(CacheKeys.Product(request.Id), token: ct);
    }
}
