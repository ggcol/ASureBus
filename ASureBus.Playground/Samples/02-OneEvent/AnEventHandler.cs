using ASureBus.Abstractions;
using ASureBus.Playground.Samples._02_OneEvent.Messages;
using Microsoft.Extensions.Logging;

namespace ASureBus.Playground.Samples._02_OneEvent;

public class AnEventHandler(ILogger<AnEventHandler> logger) : IHandleMessage<AnEvent>
{
    public Task Handle(AnEvent message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handler starts");

        logger.LogInformation("{MessageSays}", message.Something);
        
        return Task.CompletedTask;
    }
}