using ASureBus.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Playground.Samples._11_SagaTimeoutTriggeredAfterCompleting;

public class SagaTimeoutTriggeredAfterCompletingInitJob(
    IHostApplicationLifetime lifetime,
    IMessagingContext context)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await context.Send(new SagaTimeoutTriggeredAfterCompletingInitMessage(), cancellationToken).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        lifetime.StopApplication();
        return Task.CompletedTask;
    }
}

public class SagaTimeoutTriggeredAfterCompletingData : SagaData
{
}

public class SagaTimeoutTriggeredAfterCompleting(ILogger<SagaTimeoutTriggeredAfterCompleting> logger)
    : Saga<SagaTimeoutTriggeredAfterCompletingData>
        , IAmStartedBy<SagaTimeoutTriggeredAfterCompletingInitMessage>
        , IHandleMessage<SagaTimeoutTriggeredAfterCompletingAMessage>
        , IHandleTimeout<SagaTimeoutTriggeredAfterCompletingTimeout>
{
    public async Task Handle(SagaTimeoutTriggeredAfterCompletingInitMessage message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        await context.Send(new SagaTimeoutTriggeredAfterCompletingAMessage(), cancellationToken).ConfigureAwait(false);

        await RequestTimeout(context, new SagaTimeoutTriggeredAfterCompletingTimeout(), TimeSpan.FromSeconds(5), cancellationToken);
    }

    public async Task Handle(SagaTimeoutTriggeredAfterCompletingAMessage message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Completing saga after receiving message... {Time}", DateTime.UtcNow);

        await IAmComplete(cancellationToken).ConfigureAwait(false);
    }

    public async Task Handle(SagaTimeoutTriggeredAfterCompletingTimeout message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        //check console, you should see the timeout being skipped since saga is already completed
    }
}

public class SagaTimeoutTriggeredAfterCompletingInitMessage : IAmACommand
{
}

public class SagaTimeoutTriggeredAfterCompletingAMessage : IAmACommand
{
}

public class SagaTimeoutTriggeredAfterCompletingTimeout : IAmATimeout
{
}