using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Application.Features.Users.Projections;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Application.Features.Shops.Queries;

public record GetShopOwnerQuery(int ShopId, int UserId) : IRequest<UserDto>;

public class GetShopOwnerHandler(IApplicationDbContext dbContext, OpeninaryConfig config)
    : IRequestHandler<GetShopOwnerQuery, UserDto>
{
    public async Task<UserDto> Handle(GetShopOwnerQuery request, CancellationToken ct)
    {
        return await dbContext.ShopOwners
            .AsNoTracking()
            .Where(so => so.ShopId == request.ShopId && so.UserId == request.UserId)
            .Select(so => so.User)
            .ProjectToUser(config.BaseUrl)
            .FirstOrDefaultAsync(ct) ?? throw new NotFoundException("Shop owner not found.");
    }
}
