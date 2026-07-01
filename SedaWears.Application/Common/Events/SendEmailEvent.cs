namespace SedaWears.Application.Common.Events;

public record SendEmailEvent
{
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public bool UseNoReply { get; init; } = true;
}
