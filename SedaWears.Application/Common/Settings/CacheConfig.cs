namespace SedaWears.Application.Common.Settings;

public record CacheConfig
{
    public TimeSpan ProfileCacheDuration { get; init; } = TimeSpan.FromMinutes(10);
    public TimeSpan ProductCacheDuration { get; init; } = TimeSpan.FromMinutes(10);
    
    public float ProfileCacheEagerRefreshThreshold { get; init; } = 0.8f;
    public float ProductCacheEagerRefreshThreshold { get; init; } = 0.8f;
}
