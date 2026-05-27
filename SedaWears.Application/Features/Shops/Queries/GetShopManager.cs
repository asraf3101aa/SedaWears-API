using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Application.Features.Users.Projections;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Application.Features.Shops.Queries;

public record GetShopManagerQuery(int ShopId, int UserId) : IRequest<UserDto>;

public class GetShopManagerHandler(IApplicationDbContext dbContext, OpeninaryConfig config)
    : IRequestHandler<GetShopManagerQuery, UserDto>
{
    public async Task<UserDto> Handle(GetShopManagerQuery request, CancellationToken ct)
    {
        return await dbContext.ShopManagers
            .AsNoTracking()
            .Where(sm => sm.ShopId == request.ShopId && sm.UserId == request.UserId)
            .Select(sm => sm.User)
            .ProjectToUser(config.BaseUrl)
            .FirstOrDefaultAsync(ct) ?? throw new NotFoundException("Shop manager not found.");
    }
}
