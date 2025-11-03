using ASureBus.Abstractions;
using Microsoft.Extensions.Logging;
using Playground.Samples._07_DelayedAndScheduled.Messages;

namespace Playground.Samples._07_DelayedAndScheduled;

public class DelayedMessageHandler(ILogger<DelayedMessageHandler> logger)
    : IHandleMessage<DelayedMessage>
{
    public async Task Handle(DelayedMessage message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        logger.LogInformation("""
                              Delayed message created at {0} 
                              with delay: {1} 
                              received at {2}
                              {3}
                              """,
            message.CreatedAt, message.Delay, now, message.CreatedAt.Add(message.Delay) >= now);

        var delay = TimeSpan.FromSeconds(10);
        var scheduledTime = now.Add(delay);

        var scheduledMessage = new ScheduledMessage
        {
            CreatedAt = now,
            ScheduledAt = scheduledTime
        };

        await context.SendScheduled(scheduledMessage, scheduledTime, cancellationToken)
            .ConfigureAwait(false);

        // alternative way to send a scheduled message
        // await context.Send(scheduledMessage, new SendOptions
        //     {
        //         ScheduledTime = scheduledTime
        //     }, cancellationToken)
        //     .ConfigureAwait(false);
    }
}