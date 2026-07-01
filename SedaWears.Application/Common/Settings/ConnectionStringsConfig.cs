namespace SedaWears.Application.Common.Settings;

public record ConnectionStringsConfig
{
    public string Postgres { get; init; } = string.Empty;
    public string Redis { get; init; } = string.Empty;
    public string RabbitMQ { get; init; } = string.Empty;
}
