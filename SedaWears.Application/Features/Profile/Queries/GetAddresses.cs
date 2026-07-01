using SedaWears.Application.Features.Users.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common;

using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Application.Features.Profile.Queries;

public record GetAddressesQuery() : IRequest<List<AddressDto>>;

public class GetAddressesQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser, IFusionCache fusionCache) :
    IRequestHandler<GetAddressesQuery, List<AddressDto>>
{
    public async Task<List<AddressDto>> Handle(GetAddressesQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.Id ?? throw new UnauthorizedAccessException();

        return await fusionCache.GetOrSetAsync<List<AddressDto>>(
            CacheKeys.ProfileAddresses(userId),
            async (ctx, token) =>
            {
                var addresses = await dbContext.Addresses
                    .AsNoTracking()
                    .Where(a => a.UserId == userId)
                    .ToListAsync(token);

                return addresses.Select(a => new AddressDto(
                    a.Id, a.Label, a.FullName, a.Email, a.Phone, a.Street, a.City, a.ZipCode)).ToList();
            },
            token: cancellationToken);
    }
}
