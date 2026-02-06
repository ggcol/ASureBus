using ASureBus.Abstractions;
using ASureBus.Playground.Samples._01_OneCommand.Messages;
using Microsoft.Extensions.Logging;

namespace ASureBus.Playground.Samples._01_OneCommand;

public class ACommandHandler(ILogger<ACommandHandler> logger) : IHandleMessage<ACommand>
{
    public Task Handle(ACommand message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handler starts");
        logger.LogInformation("{MessageSays}", message.Something);
        return Task.CompletedTask;
    }
}