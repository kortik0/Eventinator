using System.Collections.Concurrent;

namespace WebhookReceiver.Core.DTO;

public record WebhookPayload
{
    public string Source { get; init; } = string.Empty;
    public string BodyData { get; init; } = string.Empty;
    public ConcurrentDictionary<string, string> Items { get; init; } = new();
}