using System.Text.Json;
using Microsoft.Extensions.Logging;
using WebhookReceiver.Core.DTO;
using WebhookReceiver.Core.Interfaces;
using WebhookReceiver.Core.JsonConfigurations;

namespace WebhookReceiver.Core;

public partial class ForgejoService(ILogger<ForgejoService> logger) : IWebhookParser
{
    public string ServiceName => "forgejo";

    public bool TryParse(WebhookPayload payload, out DomainEvent? domainEvent)
    {
        domainEvent = null;

        if (!payload.Items.TryGetValue("x-forgejo-event", out var rawEventType)
            || string.IsNullOrWhiteSpace(rawEventType))
        {
            LogMissingEventHeader(logger, payload.Source);
            return false;
        }

        try
        {
            LogParsingPayload(logger, payload.Source, rawEventType);

            var forgejoPayload = JsonSerializer.Deserialize(
                payload.BodyData,
                AppJsonContext.Default.ForgejoWebhookPayload);

            domainEvent = new DomainEvent(
                Guid.NewGuid(),
                payload.Source,
                rawEventType,
                payload.BodyData,
                DateTimeOffset.UtcNow);

            LogParsedSuccessfully(
                logger,
                rawEventType,
                forgejoPayload?.Repository?.FullName ?? "unknown",
                forgejoPayload?.Sender?.Login ?? "unknown");

            return true;
        }
        catch (JsonException ex)
        {
            LogJsonDeserializationError(logger, ex, rawEventType);
            return false;
        }
        catch (Exception ex)
        {
            LogParseError(logger, ex, rawEventType);
            return false;
        }
    }

    [LoggerMessage(LogLevel.Warning, "no x-forgejo-event header ({Source})")]
    static partial void LogMissingEventHeader(ILogger<ForgejoService> logger, string source);

    [LoggerMessage(LogLevel.Debug, "forgejo {EventType} from {Source}")]
    static partial void LogParsingPayload(ILogger<ForgejoService> logger, string source, string eventType);

    [LoggerMessage(LogLevel.Information, "forgejo {EventType} on {Repository} ({Sender})")]
    static partial void LogParsedSuccessfully(
        ILogger<ForgejoService> logger,
        string eventType,
        string repository,
        string sender);

    [LoggerMessage(LogLevel.Error, "bad json: forgejo {EventType}")]
    static partial void LogJsonDeserializationError(
        ILogger<ForgejoService> logger,
        Exception ex,
        string eventType);

    [LoggerMessage(LogLevel.Error, "forgejo parse error: {EventType}")]
    static partial void LogParseError(
        ILogger<ForgejoService> logger,
        Exception ex,
        string eventType);
}