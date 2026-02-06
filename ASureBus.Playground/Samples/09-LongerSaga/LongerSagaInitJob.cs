using ASureBus.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ASureBus.Playground.Samples._09_LongerSaga;

public class LongerSagaInitJob(
    IHostApplicationLifetime applicationLifetime,
    IMessagingContext context) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await context.Send(new LongerSagaInitCommand(), cancellationToken).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        applicationLifetime.StopApplication();
        return Task.CompletedTask;
    }
}

public record LongerSagaInitCommand : IAmACommand;

public class LongerSagaData : SagaData
{
    public string? SomeData { get; set; }
}

public class LongerSaga(ILogger<LongerSaga> logger) : Saga<LongerSagaData>,
    IAmStartedBy<LongerSagaInitCommand>,
    IHandleMessage<LongerSagaReply1>,
    IHandleMessage<LongerSagaReply2>,
    IHandleMessage<LongerSagaReply3>,
    IHandleMessage<LongerSagaReply4>
{
    public async Task Handle(LongerSagaInitCommand message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        SagaData.SomeData = "Lorem ";
        logger.LogInformation("{SagaData}", SagaData.SomeData);
        await context.Send(new LongerSagaMessage1(), cancellationToken).ConfigureAwait(false);
    }

    public async Task Handle(LongerSagaReply1 message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        SagaData.SomeData += "ipsum ";
        logger.LogInformation("{SagaData}", SagaData.SomeData);
        await context.Send(new LongerSagaMessage2(), cancellationToken).ConfigureAwait(false);
    }

    public async Task Handle(LongerSagaReply2 message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        SagaData.SomeData += "dolor ";
        logger.LogInformation("{SagaData}", SagaData.SomeData);
        await context.Send(new LongerSagaMessage3(), cancellationToken).ConfigureAwait(false);
    }

    public async Task Handle(LongerSagaReply3 message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        SagaData.SomeData += "sit ";
        logger.LogInformation("{SagaData}", SagaData.SomeData);
        await context.Send(new LongerSagaMessage4(), cancellationToken).ConfigureAwait(false);
    }

    public async Task Handle(LongerSagaReply4 message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        SagaData.SomeData += "amet";
        logger.LogInformation("{SagaData}", SagaData.SomeData);
        IAmComplete();
        await Task.CompletedTask;
    }
}

public record LongerSagaMessage1 : IAmACommand;

public class LongerSagaMessage1Handler(ILogger<LongerSagaMessage1Handler> logger)
    : IHandleMessage<LongerSagaMessage1>
{
    public async Task Handle(LongerSagaMessage1 message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("step 1");
        await context.Send(new LongerSagaReply1(), cancellationToken).ConfigureAwait(false);
    }
}

public record LongerSagaReply1 : IAmACommand;

public record LongerSagaMessage2 : IAmACommand;

public class LongerSagaMessage2Handler(ILogger<LongerSagaMessage2Handler> logger)
    : IHandleMessage<LongerSagaMessage2>
{
    public async Task Handle(LongerSagaMessage2 message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("step 2");
        await context.Send(new LongerSagaReply2(), cancellationToken).ConfigureAwait(false);
    }
}

public record LongerSagaReply2 : IAmACommand;

public record LongerSagaMessage3 : IAmACommand;

public class LongerSagaMessage3Handler(ILogger<LongerSagaMessage3Handler> logger)
    : IHandleMessage<LongerSagaMessage3>
{
    public async Task Handle(LongerSagaMessage3 message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("step 3");
        await context.Send(new LongerSagaReply3(), cancellationToken).ConfigureAwait(false);
    }
}

public record LongerSagaReply3 : IAmACommand;

public record LongerSagaMessage4 : IAmACommand;

public class LongerSagaMessage4Handler(ILogger<LongerSagaMessage4Handler> logger)
    : IHandleMessage<LongerSagaMessage4>
{
    public async Task Handle(LongerSagaMessage4 message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("step 4");
        await context.Send(new LongerSagaReply4(), cancellationToken).ConfigureAwait(false);
    }
}

public class LongerSagaReply4 : IAmACommand;