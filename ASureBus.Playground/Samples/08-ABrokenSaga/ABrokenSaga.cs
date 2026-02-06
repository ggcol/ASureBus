using ASureBus.Abstractions;
using ASureBus.Playground.Samples._08_ABrokenSaga.Messages;
using Microsoft.Extensions.Logging;

namespace ASureBus.Playground.Samples._08_ABrokenSaga;

public class ABrokenSaga(ILogger<ABrokenSaga> logger)
    : Saga<ABrokenSagaData>, IAmStartedBy<ABrokenSagaInitCommand>
{
    public Task Handle(ABrokenSagaInitCommand message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        SagaData.SomeData = "Some";
        
        if (message.BreakSaga) throw new Exception("Shit happens!");
        
        IAmComplete();
        return Task.CompletedTask;
    }

    public Task HandleError(Exception ex, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Any data? {IsDataPersisted}",
            !string.IsNullOrEmpty(SagaData.SomeData));
        throw ex;
    }
}

public class ABrokenSagaData : SagaData
{
    public string? SomeData { get; set; }
}