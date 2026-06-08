namespace WebhookReceiver.Core.DTO;

public record DomainEvent(
    Guid Id,
    string Source,
    string EventType,
    string Payload,
    DateTimeOffset OccuredAt
);