using System.Text.Json.Serialization;
using WebhookReceiver.Core.DTO;

namespace WebhookReceiver.Core.JsonConfigurations;

[JsonSerializable(typeof(ForgejoWebhookPayload))]
[JsonSerializable(typeof(DomainEvent))]
internal partial class AppJsonContext : JsonSerializerContext
{
}

public record ForgejoWebhookPayload(
    [property: JsonPropertyName("ref")] string? Ref = null,
    [property: JsonPropertyName("before")] string? Before = null,
    [property: JsonPropertyName("after")] string? After = null,
    [property: JsonPropertyName("commits")]
    List<Commit>? Commits = null,
    [property: JsonPropertyName("repository")]
    Repository? Repository = null,
    [property: JsonPropertyName("sender")] Sender? Sender = null
);

public record Commit(
    [property: JsonPropertyName("id")] string Id = "",
    [property: JsonPropertyName("message")]
    string Message = "",
    [property: JsonPropertyName("author")] Author? Author = null,
    [property: JsonPropertyName("timestamp")]
    DateTimeOffset Timestamp = default
);

public record Author(
    [property: JsonPropertyName("name")] string Name = "",
    [property: JsonPropertyName("email")] string Email = ""
);

public record Repository(
    [property: JsonPropertyName("name")] string Name = "",
    [property: JsonPropertyName("full_name")]
    string FullName = "",
    [property: JsonPropertyName("html_url")]
    string HtmlUrl = ""
);

public record Sender(
    [property: JsonPropertyName("login")] string Login = ""
);