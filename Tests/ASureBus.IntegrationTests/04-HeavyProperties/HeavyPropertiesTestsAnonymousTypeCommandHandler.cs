using ASureBus.Abstractions;

namespace ASureBus.IntegrationTests._04_HeavyProperties;

internal sealed class HeavyPropertiesTestsAnonymousTypeCommandHandler(CheckService checkService)
    : IHandleMessage<HeavyPropertiesTestsAnonymousTypeCommand>
{
    public Task Handle(HeavyPropertiesTestsAnonymousTypeCommand message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        var offloadType = message.OffloadedProperty.Value?.GetType();
        var offloadProps = offloadType?.GetProperties();

        var onBoardType = message.OnBoardProperty.GetType();
        var onBoardProps = onBoardType.GetProperties();

        var samePropsCount = offloadProps?.Length == onBoardProps.Length;

        var samePropsNames = offloadProps?
            .Select(p => p.Name)
            .OrderBy(n => n)
            .SequenceEqual(onBoardProps.Select(p => p.Name).OrderBy(n => n));

        if (samePropsCount && samePropsNames.HasValue && samePropsNames.Value)
        {
            checkService.Acknowledge();
        }

        checkService.IncrementProcessedMessageCount();
        return Task.CompletedTask;
    }
}