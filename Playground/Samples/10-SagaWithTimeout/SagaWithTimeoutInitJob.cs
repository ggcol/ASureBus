using ASureBus.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Playground.Samples._10_SagaWithTimeout;

public class SagaWithTimeoutInitJob(IHostApplicationLifetime lifetime, IMessagingContext context) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await context.Send(new SagaWithTimeoutInitMessage(), cancellationToken).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        lifetime.StopApplication();
        return Task.CompletedTask;
    }
}

public class SagaWithTimeoutData : SagaData
{
    public bool TimeoutTriggered { get; set; } = false;
}

public class SagaWithTimeout(ILogger<SagaWithTimeout> logger)
    : Saga<SagaWithTimeoutData>
        , IAmStartedBy<SagaWithTimeoutInitMessage>
        , IHandleMessage<SagaWithTimeoutAMessage>
        , IHandleTimeout<SagaWithTimeoutTimeout>
{
    public async Task Handle(SagaWithTimeoutInitMessage message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        await context.Send(new SagaWithTimeoutAMessage(), cancellationToken).ConfigureAwait(false);

        await RequestTimeout(context, new SagaWithTimeoutTimeout(), TimeSpan.FromSeconds(5), cancellationToken);
    }

    public async Task Handle(SagaWithTimeoutAMessage message, IMessagingContext context, CancellationToken cancellationToken)
    {
        while (!SagaData.TimeoutTriggered)
        {
            logger.LogInformation("Waiting for timeout to trigger... {Time}", DateTime.UtcNow);
            Thread.Sleep(1000);
        }
        
        logger.LogInformation("Timeout triggered, completing saga... {Time}", DateTime.UtcNow);
        
        await IAmComplete(cancellationToken).ConfigureAwait(false);
    }

    public async Task Handle(SagaWithTimeoutTimeout message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        SagaData.TimeoutTriggered = true;
    }
}

public class SagaWithTimeoutInitMessage : IAmACommand
{
}

public class SagaWithTimeoutAMessage : IAmACommand
{
}

public class SagaWithTimeoutTimeout : IAmATimeout
{
}