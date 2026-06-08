using WebhookReceiver.Core.Interfaces;

namespace WebhookReceiver.Middlewares;

public class HeadersMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context, IBusinessContext businessContext)
    {
        foreach (var header in context.Request.Headers)
        {
            if (header.Key.StartsWith("X-", StringComparison.InvariantCultureIgnoreCase))
            {
                businessContext.Set(header.Key, header.Value!);
            }
        }

        await next(context);
    }
}