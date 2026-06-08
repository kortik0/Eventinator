using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using NATS.Net;
using Scalar.AspNetCore;
using WebhookReceiver.Backgrounds;
using WebhookReceiver.Channels;
using WebhookReceiver.Core;
using WebhookReceiver.Core.DTO;
using WebhookReceiver.Core.Fabrics;
using WebhookReceiver.Core.Interfaces;
using WebhookReceiver.Core.Options;
using WebhookReceiver.Core.Services;
using WebhookReceiver.HealthChecks;
using WebhookReceiver.Middlewares;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.Configure<NatsOptions>(builder.Configuration.GetSection(NatsOptions.SectionName));
builder.Services.AddSingleton(sp => new NatsClient(
    sp.GetRequiredService<IOptions<NatsOptions>>().Value.Url));

builder.Services.AddSingleton<WebhookChannel>();
builder.Services.AddHostedService<WebhookJobProcessor>();

builder.Services.AddScoped<IBusinessContext, BusinessContext>();
builder.Services.AddScoped<IWebhookParser, ForgejoService>();
builder.Services.AddScoped<ServiceFabrics>();
builder.Services.AddScoped<WebhookIngestionService>();
builder.Services.AddScoped<IEventPublisher, NatsSenderService>();

builder.Services.AddHealthChecks()
    .AddCheck<NatsHealthCheck>("nats", tags: ["ready"]);

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<HeadersMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapHealthChecks("/healthz", new HealthCheckOptions { AllowCachingResponses = false });

app.MapPost("webhooks/{source}",
    async (string source, HttpRequest request, WebhookChannel channel, IBusinessContext context) =>
    {
        using var reader = new StreamReader(request.Body);
        var data = await reader.ReadToEndAsync();

        var payload = new WebhookPayload
        {
            Source = source,
            BodyData = data,
            Items = context.Items,
        };

        await channel.Writer.WriteAsync(payload);

        return Results.Accepted();
    });

app.Run();