using System.Threading.Channels;
using WebhookReceiver.Core.DTO;

namespace WebhookReceiver.Channels;

public class WebhookChannel
{
    private readonly Channel<WebhookPayload> _channel = Channel.CreateUnbounded<WebhookPayload>();

    public ChannelReader<WebhookPayload> Reader => _channel.Reader;
    public ChannelWriter<WebhookPayload> Writer => _channel.Writer;
}