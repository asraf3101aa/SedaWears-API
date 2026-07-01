using Microsoft.Extensions.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Application.Features.Users.Projections;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Common;

using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Application.Features.Profile.Queries;

public record GetMeQuery : IRequest<UserDto>;

public class GetMeHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IFusionCache fusionCache,
    IOptions<OpeninaryConfig> configOptions,
    IOptions<CacheConfig> cacheConfigOptions) : IRequestHandler<GetMeQuery, UserDto>
{
    public async Task<UserDto> Handle(GetMeQuery request, CancellationToken ct)
    {
        var userId = currentUser.Id ?? throw new UnauthorizedAccessException("User is not authenticated.");

        return await fusionCache.GetOrSetAsync<UserDto>(
            CacheKeys.Profile(userId),
            async (ctx, token) =>
            {
                return await dbContext.Users
                    .AsNoTracking()
                    .Where(u => u.Id == userId)
                    .ProjectToUser(configOptions.Value.BaseUrl)
                    .FirstOrDefaultAsync(token)
                    ?? throw new UnauthorizedAccessException("User not found.");
            },
            new FusionCacheEntryOptions
            {
                Duration = cacheConfigOptions.Value.ProfileCacheDuration
            },
            token: ct);
    }
}
