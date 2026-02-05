using ASureBus.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Playground.Samples._13_UseConsumerScopedQueueForTopics;

public class UseConsumerScopedQueueForTopicsInitJob(
    IHostApplicationLifetime lifetime,
    ILogger<UseConsumerScopedQueueForTopicsInitJob> logger,
    IMessagingContext context) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await context.Publish(new UseConsumerScopedQueueForTopicsEvent(), cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Published UseConsumerScopedQueueForTopicsEvent");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping...");
        lifetime.StopApplication();
        return Task.CompletedTask;
    }
}

public class UseConsumerScopedQueueForTopicsEventHandler(ILogger<UseConsumerScopedQueueForTopicsEventHandler> logger)
    : IHandleMessage<UseConsumerScopedQueueForTopicsEvent>
{
    public Task Handle(UseConsumerScopedQueueForTopicsEvent message, IMessagingContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling event: {EventName}", nameof(UseConsumerScopedQueueForTopicsEvent));
        return Task.CompletedTask;
    }
}

public record UseConsumerScopedQueueForTopicsEvent : IAmAnEvent;