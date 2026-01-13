using ASureBus.Abstractions;

namespace ASureBus.IntegrationTests._03_MessageLockRenewal;

public class MessageLockRenewalLongRunningCommandHandler(CheckService checkService)
    : IHandleMessage<MessageLockRenewalLongRunningCommand>
{
    public async Task Handle(MessageLockRenewalLongRunningCommand message, IMessagingContext context, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        
        await Task.Delay(TimeSpan.FromSeconds(message.ProcessingTimeInSeconds), cancellationToken);
        
        var endTime = DateTime.UtcNow;
        var actualProcessingTime = (endTime - startTime).TotalSeconds;
        
        checkService.Acknowledge(actualProcessingTime);
        checkService.IncrementProcessedMessageCount();
    }
}