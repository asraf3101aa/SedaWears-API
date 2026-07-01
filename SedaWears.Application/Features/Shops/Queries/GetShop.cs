using Microsoft.Extensions.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Features.Shops.Models;
using SedaWears.Application.Features.Shops.Projections;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Application.Features.Shops.Queries;

public record GetShopQuery(int Id) : IRequest<ShopDto>;

public class GetShopHandler(IApplicationDbContext dbContext, IOptions<OpeninaryConfig> configOptions) : IRequestHandler<GetShopQuery, ShopDto>
{
    public async Task<ShopDto> Handle(GetShopQuery request, CancellationToken ct)
    {
        return await dbContext.Shops
            .AsNoTracking()
            .Where(s => s.Id == request.Id)
            .ProjectToShop(configOptions.Value.BaseUrl)
            .FirstOrDefaultAsync(ct) ?? throw new ShopNotFoundException();
    }
}
