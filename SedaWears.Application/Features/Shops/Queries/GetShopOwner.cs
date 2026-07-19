using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Application.Features.Users.Projections;

namespace SedaWears.Application.Features.Shops.Queries;

public record GetShopOwnerQuery(int ShopId, int UserId) : IRequest<UserDto>;

public class GetShopOwnerHandler(IApplicationDbContext dbContext, IOptions<OpeninaryConfig> configOptions) : IRequestHandler<GetShopOwnerQuery, UserDto>
{
    public async Task<UserDto> Handle(GetShopOwnerQuery request, CancellationToken ct)
        => await dbContext.ShopOwners
            .AsNoTracking()
            .Where(so => so.ShopId == request.ShopId && so.UserId == request.UserId)
            .Select(so => so.User)
            .ProjectToUser(configOptions.Value.BaseUrl, dbContext)
            .FirstOrDefaultAsync(ct) ?? throw new ShopNotFoundException();
}
