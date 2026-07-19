using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Application.Features.Users.Projections;

namespace SedaWears.Application.Features.Shops.Queries;

public record GetShopManagerQuery(int ShopId, int UserId) : IRequest<UserDto>;

public class GetShopManagerHandler(IApplicationDbContext dbContext, IOptions<OpeninaryConfig> configOptions) : IRequestHandler<GetShopManagerQuery, UserDto>
{
    public async Task<UserDto> Handle(GetShopManagerQuery request, CancellationToken ct)
        => await dbContext.ShopManagers
            .AsNoTracking()
            .Where(sm => sm.ShopId == request.ShopId && sm.UserId == request.UserId)
            .Select(sm => sm.User)
            .ProjectToUser(configOptions.Value.BaseUrl, dbContext)
            .FirstOrDefaultAsync(ct) ?? throw new ShopNotFoundException();
}
