using ASureBus.Abstractions;

namespace ASureBus.IntegrationTests._04_HeavyProperties;

internal sealed class HeavyPropertiesTestsBasicTypeCommandHandler(CheckService checkService)
    : IHandleMessage<HeavyPropertiesTestsBasicTypeCommand>
{
    public Task Handle(HeavyPropertiesTestsBasicTypeCommand message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        if (message.OnBoardProperty.Equals(message.OffloadedProperty.Value))
        {
            checkService.Acknowledge();
        }

        checkService.IncrementProcessedMessageCount();
        return Task.CompletedTask;
    }
}