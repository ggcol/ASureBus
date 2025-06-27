namespace ASureBus.Core.TypesHandling.Entities;

internal sealed class SagaHandlerType : ListenerType
{
    internal required bool IsInitMessageHandler { get; init; }
    internal required bool IsTimeoutHandler { get; init; }
}