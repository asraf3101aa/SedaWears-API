using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Application.Common;

public static class CachePolicies
{
    public static readonly FusionCacheEntryOptions UserProfile = new()
    {
        Duration = TimeSpan.FromMinutes(10),
        EagerRefreshThreshold = 0.8f
    };

    public static readonly FusionCacheEntryOptions UserAddresses = new()
    {
        Duration = TimeSpan.FromMinutes(10),
        EagerRefreshThreshold = 0.8f
    };

    public static readonly FusionCacheEntryOptions Product = new()
    {
        Duration = TimeSpan.FromMinutes(10),
        EagerRefreshThreshold = 0.8f
    };

    public static readonly FusionCacheEntryOptions Shop = new()
    {
        Duration = TimeSpan.FromMinutes(10),
        EagerRefreshThreshold = 0.8f
    };

    public static readonly FusionCacheEntryOptions Category = new()
    {
        Duration = TimeSpan.FromMinutes(10),
        EagerRefreshThreshold = 0.8f
    };
}
