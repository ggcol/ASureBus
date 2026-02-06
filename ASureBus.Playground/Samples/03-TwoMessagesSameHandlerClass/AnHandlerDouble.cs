using ASureBus.Abstractions;
using ASureBus.Playground.Samples._03_TwoMessagesSameHandlerClass.Messages;
using Microsoft.Extensions.Logging;

namespace ASureBus.Playground.Samples._03_TwoMessagesSameHandlerClass;

public class AnHandlerDouble(ILogger<AnHandlerDouble> logger) 
    : IHandleMessage<Message1>, IHandleMessage<Message2>
{
    public Task Handle(Message1 message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handler for {Message} starts", message.GetType().Name);

        logger.LogInformation("{MessageSays}", message.Something);
        
        return Task.CompletedTask;
    }

    public Task Handle(Message2 message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handler for {Message} starts", message.GetType().Name);

        logger.LogInformation("{MessageSays}", message.Something);
        
        return Task.CompletedTask;
    }
}