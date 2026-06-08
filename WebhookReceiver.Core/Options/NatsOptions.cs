namespace WebhookReceiver.Core.Options;

public sealed class NatsOptions
{
    public const string SectionName = "Nats";

    public string Url { get; init; } = "nats://localhost:4222";
    public string SubjectPrefix { get; init; } = "webhooks";
}
