
namespace SedaWears.Application.Common.Settings;

public record AppConfig
{
    public string CustomerFrontendUrl { get; init; } = string.Empty;
    public string AdminFrontendUrl { get; init; } = string.Empty;
    public string ManagerFrontendUrl { get; init; } = string.Empty;
    public string OwnerFrontendUrl { get; init; } = string.Empty;
    public string CookieDomain { get; init; } = string.Empty;
    public CorsConfig Cors { get; init; } = new();
    public double AdminInvitationExpiry { get; init; } = 24.0;
    public double OwnerInvitationExpiry { get; init; } = 72.0;
    public double ManagerInvitationExpiry { get; init; } = 72.0;
}

public record CorsConfig
{
    public string AllowedOrigins { get; init; } = string.Empty;
}
