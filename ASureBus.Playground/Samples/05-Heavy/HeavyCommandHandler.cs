using ASureBus.Abstractions;
using ASureBus.Playground.Samples._05_Heavy.Messages;
using Microsoft.Extensions.Logging;

namespace ASureBus.Playground.Samples._05_Heavy;

public class HeavyCommandHandler(ILogger<HeavyCommandHandler> logger) : IHandleMessage<HeavyCommand>
{
    public Task Handle(HeavyCommand message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("{MessageSays}", message.AHeavyProp?.Value);
        return Task.CompletedTask;
    }
}