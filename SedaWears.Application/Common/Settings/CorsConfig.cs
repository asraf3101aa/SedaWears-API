namespace SedaWears.Application.Common.Settings;

public record CorsConfig
{
    public string[] AllowedOrigins { get; init; } = [];
}
