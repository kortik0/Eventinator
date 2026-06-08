using Microsoft.Extensions.Diagnostics.HealthChecks;
using NATS.Net;

namespace WebhookReceiver.HealthChecks;

public sealed class NatsHealthCheck(NatsClient natsClient) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await natsClient.PingAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(exception: ex);
        }
    }
}