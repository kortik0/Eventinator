using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Net;
using WebhookReceiver.Core.DTO;
using WebhookReceiver.Core.Interfaces;
using WebhookReceiver.Core.JsonConfigurations;
using WebhookReceiver.Core.Options;

namespace WebhookReceiver.Core.Services;

public partial class NatsSenderService(
    ILogger<NatsSenderService> logger,
    NatsClient natsClient,
    IOptions<NatsOptions> options) : IEventPublisher
{
    private readonly NatsOptions _options = options.Value;

    public async Task PublishAsync(DomainEvent @event, CancellationToken cancellationToken = default)
    {
        var subject = BuildSubject(@event);
        var payload = JsonSerializer.Serialize(@event, AppJsonContext.Default.DomainEvent);

        await natsClient.PublishAsync(subject, payload, cancellationToken: cancellationToken);

        LogPublished(logger, @event.Id, subject);
    }

    private string BuildSubject(DomainEvent @event)
    {
        var source = Sanitize(@event.Source);
        var eventType = Sanitize(@event.EventType);
        return $"{_options.SubjectPrefix}.{source}.{eventType}";
    }

    private static string Sanitize(string value) =>
        value.ToLowerInvariant().Replace(' ', '_');

    [LoggerMessage(LogLevel.Debug, "nats {WebhookId} -> {Subject}")]
    static partial void LogPublished(ILogger<NatsSenderService> logger, Guid webhookId, string subject);
}