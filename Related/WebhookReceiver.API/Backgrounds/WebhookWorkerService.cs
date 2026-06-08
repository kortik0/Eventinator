using WebhookReceiver.Channels;
using WebhookReceiver.Core.Services;

namespace WebhookReceiver.Backgrounds;

public partial class WebhookJobProcessor(
    ILogger<WebhookJobProcessor> logger,
    WebhookChannel channel,
    IServiceScopeFactory serviceScopeFactory)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogStarting(logger);

        await foreach (var payload in channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var ingestionService = scope.ServiceProvider.GetRequiredService<WebhookIngestionService>();

                using (logger.BeginScope(new Dictionary<string, object> { ["Source"] = payload.Source }))
                {
                    LogProcessing(logger, payload.Source);
                    await ingestionService.IngestAsync(payload, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                LogUnhandledError(logger, ex, payload.Source);
            }
        }

        LogStopping(logger);
    }

    [LoggerMessage(LogLevel.Information, "worker up")]
    static partial void LogStarting(ILogger<WebhookJobProcessor> logger);

    [LoggerMessage(LogLevel.Debug, "dequeue {Source}")]
    static partial void LogProcessing(ILogger<WebhookJobProcessor> logger, string source);

    [LoggerMessage(LogLevel.Error, "worker blew up on {Source}")]
    static partial void LogUnhandledError(ILogger<WebhookJobProcessor> logger, Exception ex, string source);

    [LoggerMessage(LogLevel.Information, "worker down")]
    static partial void LogStopping(ILogger<WebhookJobProcessor> logger);
}