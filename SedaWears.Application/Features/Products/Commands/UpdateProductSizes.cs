using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Application.Features.Products.Models;
using SedaWears.Application.Common;
using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Application.Features.Products.Commands;

public record UpdateProductSizesCommand(int Id, List<ProductSizeDto>? Sizes) : IRequest<Unit>;

public class UpdateProductSizesValidator : AbstractValidator<UpdateProductSizesCommand>
{
    public UpdateProductSizesValidator()
    {

        RuleFor(x => x.Sizes)
            .NotEmpty().WithMessage("At least one size with stock must be provided.");
    }
}

public class UpdateProductSizesHandler(IApplicationDbContext dbContext, IFusionCache fusionCache) : IRequestHandler<UpdateProductSizesCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProductSizesCommand request, CancellationToken ct)
    {
        var product = await dbContext.Products
            .Include(p => p.SizeStocks)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct) ?? throw new ProductNotFoundException();

        // Sync sizes
        product.SizeStocks.Clear();
        foreach (var s in request.Sizes!)
        {
            product.SizeStocks.Add(new ProductSizeStock { Size = s.Size, Stock = s.Stock });
        }

        await dbContext.SaveChangesAsync(ct);
        await fusionCache.RemoveAsync(CacheKeys.Product(request.Id), token: ct);
        return Unit.Value;
    }
}
