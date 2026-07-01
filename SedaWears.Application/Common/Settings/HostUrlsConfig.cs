namespace SedaWears.Application.Common.Settings;

public record HostUrlsConfig
{
    public string Customer { get; init; } = string.Empty;
    public string Admin { get; init; } = string.Empty;
    public string Manager { get; init; } = string.Empty;
    public string Owner { get; init; } = string.Empty;
}
