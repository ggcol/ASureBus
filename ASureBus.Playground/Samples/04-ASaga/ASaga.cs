using ASureBus.Abstractions;
using ASureBus.Playground.Samples._04_ASaga.Messages;
using Microsoft.Extensions.Logging;

namespace ASureBus.Playground.Samples._04_ASaga;

public class ASaga(ILogger<ASaga> logger) : Saga<ASagaData>,
    IAmStartedBy<ASagaInitCommand>,
    IHandleMessage<AReply>
{
    public async Task Handle(ASagaInitCommand message,
        IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        var messageName = message.GetType().Name;

        logger.LogInformation("========== {MessageName} ==========", messageName);

        logger.LogInformation("{MessageName} received, correlationId: {CorrelationId}",
            messageName, context.CorrelationId);

        await context.Send(new AnotherCommand
            {
                Something = "Hello world!"
            }, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Handle(AReply message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        var messageName = message.GetType().Name;

        logger.LogInformation("========== {MessageName} ==========", messageName);

        logger.LogInformation("{MessageName} received, correlationId: {CorrelationId}",
            messageName, context.CorrelationId);

        logger.LogInformation("{MessageName} says: {Something}",
            messageName, message.Something);

        IAmComplete();
    }
}