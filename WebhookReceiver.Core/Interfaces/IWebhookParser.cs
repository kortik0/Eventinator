using System.Collections.Concurrent;
using WebhookReceiver.Core.DTO;

namespace WebhookReceiver.Core.Interfaces;

public interface IWebhookParser
{
    string ServiceName { get; }

    bool TryParse(WebhookPayload payload, out DomainEvent? domainEvent);
}

public interface IBusinessContext
{
    ConcurrentDictionary<string, string> Items { get; }
    string? Get(string key);
    void Set(string key, string value);
}

public class BusinessContext : IBusinessContext
{
    public ConcurrentDictionary<string, string> Items { get; } = new();

    public string? Get(string key) =>
        Items.GetValueOrDefault(key.ToLowerInvariant());

    public void Set(string key, string value) =>
        Items[key.ToLowerInvariant()] = value;
}