using FluentAssertions;
using Moq;
using WebhookReceiver.Core.DTO;
using WebhookReceiver.Core.Fabrics;
using WebhookReceiver.Core.Interfaces;
using Xunit;

namespace WebhookReceiver.Tests;

public sealed class ServiceFabricsTests
{
    [Fact]
    public void Resolves_parser_case_insensitive()
    {
        var expected = new DomainEvent(Guid.NewGuid(), "forgejo", "push", "{}", DateTimeOffset.UtcNow);
        var sut = new ServiceFabrics([BuildParser("Forgejo", expected).Object]);

        sut.GetDomainEventByServiceParser(new WebhookPayload { Source = "forgejo" }).Should().Be(expected);
        sut.GetDomainEventByServiceParser(new WebhookPayload { Source = "FORGEJO" }).Should().Be(expected);
    }

    [Fact]
    public void Picks_right_parser_when_several_registered()
    {
        var forgejoEvent = new DomainEvent(Guid.NewGuid(), "forgejo", "push", "{}", DateTimeOffset.UtcNow);
        var githubEvent = new DomainEvent(Guid.NewGuid(), "github", "push", "{}", DateTimeOffset.UtcNow);

        var forgejoParser = BuildParser("Forgejo", forgejoEvent);
        var githubParser = BuildParser("GitHub", githubEvent);
        var sut = new ServiceFabrics([forgejoParser.Object, githubParser.Object]);

        sut.GetDomainEventByServiceParser(new WebhookPayload { Source = "github" }).Should().Be(githubEvent);
        forgejoParser.Verify(p => p.TryParse(It.IsAny<WebhookPayload>(), out It.Ref<DomainEvent?>.IsAny), Times.Never);
    }

    [Fact]
    public void Throws_on_unknown_source()
    {
        var sut = new ServiceFabrics([BuildParser("Forgejo",
            new DomainEvent(Guid.NewGuid(), "forgejo", "push", "{}", DateTimeOffset.UtcNow)).Object]);

        var act = () => sut.GetDomainEventByServiceParser(new WebhookPayload { Source = "unknown-source" });

        act.Should().Throw<InvalidOperationException>().WithMessage("*unknown-source*");
    }

    [Fact]
    public void Throws_when_no_parsers()
    {
        var act = () => new ServiceFabrics([]).GetDomainEventByServiceParser(new WebhookPayload { Source = "forgejo" });
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Throws_when_parser_returns_false()
    {
        var parser = new Mock<IWebhookParser>();
        parser.Setup(p => p.ServiceName).Returns("Forgejo");

        DomainEvent? nullOut = null;
        parser.Setup(p => p.TryParse(It.IsAny<WebhookPayload>(), out nullOut)).Returns(false);

        var act = () => new ServiceFabrics([parser.Object])
            .GetDomainEventByServiceParser(new WebhookPayload { Source = "forgejo" });

        act.Should().Throw<InvalidOperationException>().WithMessage("*Forgejo*");
    }

    private static Mock<IWebhookParser> BuildParser(string serviceName, DomainEvent returns)
    {
        var mock = new Mock<IWebhookParser>();
        mock.Setup(p => p.ServiceName).Returns(serviceName);

        DomainEvent? captured = returns;
        mock.Setup(p => p.TryParse(It.IsAny<WebhookPayload>(), out captured)).Returns(true);

        return mock;
    }
}
