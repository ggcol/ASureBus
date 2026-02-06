using ASureBus.Abstractions;
using ASureBus.Playground.Samples._04_ASaga.Messages;
using Microsoft.Extensions.Logging;

namespace ASureBus.Playground.Samples._04_ASaga;

public class AnotherCommandHandler(ILogger<AnotherCommandHandler> logger)
    : IHandleMessage<AnotherCommand>
{
    public async Task Handle(AnotherCommand message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        var messageName = message.GetType().Name;

        logger.LogInformation("========== {MessageName} ==========", messageName);
        
        logger.LogInformation("{MessageName} received, correlationId: {CorrelationId}",
            messageName, context.CorrelationId);
        
        logger.LogInformation("{MessageName} says: {MessageSays}",
            messageName, context.CorrelationId);

        await context.Send(new AReply
            {
                Something = message.Something
            }, cancellationToken)
            .ConfigureAwait(false);
    }
}