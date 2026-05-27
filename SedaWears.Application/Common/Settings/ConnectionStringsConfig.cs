namespace SedaWears.Application.Common.Settings;

public record ConnectionStringsConfig
{
    public string Postgres { get; init; } = string.Empty;
    public string Redis { get; init; } = string.Empty;
    public string RedisCertThumbprint { get; init; } = string.Empty;
}
