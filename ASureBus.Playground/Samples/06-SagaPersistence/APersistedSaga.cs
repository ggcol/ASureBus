using ASureBus.Abstractions;
using ASureBus.Playground.Samples._06_SagaPersistence.Messages;
using Microsoft.Extensions.Logging;

namespace ASureBus.Playground.Samples._06_SagaPersistence;

public class APersistedSaga(ILogger<APersistedSaga> logger) : Saga<APersistedSagaData>,
    IAmStartedBy<APersistedSagaInitCommand>,
    IHandleMessage<APersistedSagaReply>
{
    public async Task Handle(APersistedSagaInitCommand message,
        IMessagingContext context,
        CancellationToken cancellationToken)
    {
        var messageName = message.GetType().Name;

        SagaData.AProp = "Hello";
        
        logger.LogInformation("========== {MessageName} ==========", messageName);
        
        logger.LogInformation("{MessageName} received, correlationId: {CorrelationId}", 
            messageName, context.CorrelationId);

        await context.Send(new APersistedSagaCommand(), cancellationToken)
            .ConfigureAwait(false);
    }

    public Task Handle(APersistedSagaReply message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        var messageName = message.GetType().Name;
        
        SagaData.AProp += $" {message.Something}";

        logger.LogInformation("========== {MessageName} ==========", messageName);

        var correlationId = context.CorrelationId;
        logger.LogInformation("{MessageName} received, correlationId: {CorrelationId}", 
            messageName, correlationId);

        logger.LogInformation("Print saga data: {SagaData}", SagaData.AProp);

        IAmComplete();
        
        logger.LogInformation("Saga completed, correlationId: {CorrelationId}", correlationId);
        return Task.CompletedTask;
    }
}