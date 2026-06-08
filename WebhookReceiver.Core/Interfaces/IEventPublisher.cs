using WebhookReceiver.Core.DTO;

namespace WebhookReceiver.Core.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync(DomainEvent @event, CancellationToken cancellationToken = default);
}
