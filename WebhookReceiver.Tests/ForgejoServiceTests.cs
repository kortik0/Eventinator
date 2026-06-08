using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WebhookReceiver.Core;
using WebhookReceiver.Core.DTO;
using Xunit;

namespace WebhookReceiver.Tests;

public sealed class ForgejoServiceTests
{
    private readonly ForgejoService _sut = new(Mock.Of<ILogger<ForgejoService>>());

    [Theory]
    [InlineData("push")]
    [InlineData("pull_request")]
    [InlineData("issues")]
    [InlineData("totally_unknown_event")]
    public void Parses_valid_payload(string eventType)
    {
        var payload = BuildPayload(eventType);

        var ok = _sut.TryParse(payload, out var ev);

        ok.Should().BeTrue();
        ev.Should().NotBeNull();
        ev!.EventType.Should().Be(eventType);
        ev.Source.Should().Be("forgejo");
        ev.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Rejects_missing_or_empty_event_header()
    {
        var noHeader = new WebhookPayload { Source = "forgejo", BodyData = MinimalBody() };
        _sut.TryParse(noHeader, out var ev1).Should().BeFalse();
        ev1.Should().BeNull();

        _sut.TryParse(BuildPayload(""), out var ev2).Should().BeFalse();
        ev2.Should().BeNull();
    }

    [Fact]
    public void Rejects_bad_json()
    {
        var payload = BuildPayload("push", body: "NOT_VALID_JSON{{{");

        _sut.TryParse(payload, out var ev).Should().BeFalse();
        ev.Should().BeNull();
    }

    [Fact]
    public void Accepts_empty_json_object()
    {
        _sut.TryParse(BuildPayload("push", body: "{}"), out var ev).Should().BeTrue();
        ev.Should().NotBeNull();
    }

    private static WebhookPayload BuildPayload(string eventType, string? body = null)
    {
        var items = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(eventType))
            items["x-forgejo-event"] = eventType;

        return new WebhookPayload
        {
            Source = "forgejo",
            BodyData = body ?? MinimalBody(),
            Items = items,
        };
    }

    private static string MinimalBody() =>
        """
        {
          "ref": "refs/heads/main",
          "sender": { "login": "octocat" },
          "repository": { "name": "repo", "full_name": "octocat/repo", "html_url": "https://forgejo.example.com/octocat/repo" }
        }
        """;
}
