using WebhookReceiver.Core.DTO;
using WebhookReceiver.Core.Interfaces;

namespace WebhookReceiver.Core.Fabrics;

public class ServiceFabrics(IEnumerable<IWebhookParser> webhookParsers)
{
    private readonly List<IWebhookParser> _webhookParsers = webhookParsers.ToList();

    public DomainEvent GetDomainEventByServiceParser(WebhookPayload payload)
    {
        var parser = _webhookParsers.FirstOrDefault(p =>
                         p.ServiceName.Equals(payload.Source, StringComparison.OrdinalIgnoreCase))
                     ?? throw new InvalidOperationException(
                         $"No webhook parser registered for source '{payload.Source}'.");

        if (!parser.TryParse(payload, out var domainEvent) || domainEvent is null)
            throw new InvalidOperationException(
                $"Parser '{parser.ServiceName}' failed to process the webhook payload.");

        return domainEvent;
    }
}