using Microsoft.Extensions.Logging;
using WebhookReceiver.Core.DTO;
using WebhookReceiver.Core.Fabrics;
using WebhookReceiver.Core.Interfaces;

namespace WebhookReceiver.Core.Services;

public partial class WebhookIngestionService(
    ILogger<WebhookIngestionService> logger,
    ServiceFabrics serviceFabrics,
    IEventPublisher eventPublisher)
{
    public async Task IngestAsync(WebhookPayload payload, CancellationToken cancellationToken = default)
    {
        LogIngesting(logger, payload.Source);

        DomainEvent domainEvent;
        try
        {
            domainEvent = serviceFabrics.GetDomainEventByServiceParser(payload);
        }
        catch (Exception ex)
        {
            LogParseFailure(logger, ex, payload.Source);
            return;
        }

        using (logger.BeginScope(new Dictionary<string, object>
               {
                   ["WebhookId"] = domainEvent.Id.ToString(),
                   ["Source"] = domainEvent.Source,
                   ["EventType"] = domainEvent.EventType,
               }))
        {
            try
            {
                await eventPublisher.PublishAsync(domainEvent, cancellationToken);
                LogPublished(logger, domainEvent.Id, domainEvent.EventType, domainEvent.Source);
            }
            catch (Exception ex)
            {
                LogPublishFailure(logger, ex, domainEvent.Id, domainEvent.Source);
            }
        }
    }

    [LoggerMessage(LogLevel.Information, "ingesting {Source}")]
    static partial void LogIngesting(ILogger<WebhookIngestionService> logger, string source);

    [LoggerMessage(LogLevel.Error, "parse failed for {Source}")]
    static partial void LogParseFailure(ILogger<WebhookIngestionService> logger, Exception ex, string source);

    [LoggerMessage(LogLevel.Information, "published {WebhookId} {EventType} from {Source}")]
    static partial void LogPublished(
        ILogger<WebhookIngestionService> logger, Guid webhookId, string eventType, string source);

    [LoggerMessage(LogLevel.Error, "publish failed for {WebhookId} ({Source})")]
    static partial void LogPublishFailure(
        ILogger<WebhookIngestionService> logger, Exception ex, Guid webhookId, string source);
}