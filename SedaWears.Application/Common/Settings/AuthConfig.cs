namespace SedaWears.Application.Common.Settings;

public record AuthConfig
{
    public string CookieDomain { get; init; } = string.Empty;
    public double AdminInvitationExpiry { get; init; } = 24.0;
    public double OwnerInvitationExpiry { get; init; } = 72.0;
    public double ManagerInvitationExpiry { get; init; } = 72.0;
}
