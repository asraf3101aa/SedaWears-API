using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Application.Features.Users.Projections;

namespace SedaWears.Application.Features.Shops.Queries;

public record GetShopMemberQuery(int ShopId, int UserId) : IRequest<UserDto>;

public class GetShopMemberHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetShopMemberQuery, UserDto>
{
    public async Task<UserDto> Handle(GetShopMemberQuery request, CancellationToken ct)
    {
        var owner = await dbContext.ShopOwners
            .AsNoTracking()
            .Where(so => so.ShopId == request.ShopId && so.UserId == request.UserId)
            .Select(so => so.User)
            .ProjectToUser()
            .FirstOrDefaultAsync(ct);

        if (owner != null) return owner;

        var manager = await dbContext.ShopManagers
            .AsNoTracking()
            .Where(sm => sm.ShopId == request.ShopId && sm.UserId == request.UserId)
            .Select(sm => sm.User)
            .ProjectToUser()
            .FirstOrDefaultAsync(ct);

        if (manager != null) return manager;

        throw new NotFoundException("Shop member not found.");
    }
}
