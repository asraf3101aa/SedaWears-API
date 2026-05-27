using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Features.Products.Models;
using SedaWears.Application.Features.Products.Projections;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Application.Features.Products.Queries;

public record GetProductQuery(int Id) : IRequest<ProductDto>;

public class GetProductHandler(IApplicationDbContext dbContext, OpeninaryConfig config) : IRequestHandler<GetProductQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductQuery request, CancellationToken ct)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .ProjectToProduct(config.BaseUrl)
            .FirstOrDefaultAsync(ct) ?? throw new NotFoundException("Product not found");
    }
}
