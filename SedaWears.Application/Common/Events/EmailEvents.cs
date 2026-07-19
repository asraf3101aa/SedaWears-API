namespace SedaWears.Application.Common.Events;

public record ManagerInvitationEmailEvent
{
    public string To { get; init; } = string.Empty;
    public string ShopName { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}

public record OwnerInvitationEmailEvent
{
    public string To { get; init; } = string.Empty;
    public string ShopName { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}

public record AdminInvitationEmailEvent
{
    public string To { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}

public record ForgotPasswordEmailEvent
{
    public string To { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}
