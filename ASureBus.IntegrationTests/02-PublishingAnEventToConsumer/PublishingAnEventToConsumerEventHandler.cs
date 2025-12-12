using ASureBus.Abstractions;

namespace ASureBus.IntegrationTests._02_PublishingAnEventToConsumer;

internal class PublishingAnEventToConsumerEventHandler(CheckService checkService) : IHandleMessage<PublishingAnEventToConsumerEvent>
{
    public Task Handle(PublishingAnEventToConsumerEvent messageToHandler, IMessagingContext context, CancellationToken cancellationToken)
    {
        checkService.Acknowledged = true;
        return Task.CompletedTask;
    }
}