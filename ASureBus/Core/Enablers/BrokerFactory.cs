using ASureBus.Abstractions;
using ASureBus.Core.Messaging;
using ASureBus.Core.TypesHandling.Entities;
using ASureBus.IO.Heavies;
using Microsoft.Extensions.DependencyInjection;

namespace ASureBus.Core.Enablers;

internal interface IBrokerFactory
{
    internal IHandlerBroker Get(IServiceProvider serviceProvider,
        HandlerType handlerType, Guid? correlationId = null);

    internal ISagaBroker Get(IServiceProvider serviceProvider,
        SagaType sagaType, object? implSaga, ListenerType listenerType);
}

internal class BrokerFactory(IHeavyIO heavyIO) : IBrokerFactory
{
    public IHandlerBroker Get(IServiceProvider serviceProvider,
        HandlerType handlerType, Guid? correlationId = null)
    {
        var implListener = ActivatorUtilities.CreateInstance(
            serviceProvider, handlerType.Type);

        var brokerImplType = typeof(HandlerBroker<>)
            .MakeGenericType(handlerType.MessageType.Type);

        var context = new MessagingContextInternal(heavyIO);

        if (correlationId is not null)
        {
            context.CorrelationId = correlationId.Value;
        }

        return (IHandlerBroker)ActivatorUtilities.CreateInstance(
            serviceProvider, brokerImplType, implListener, context);
    }

    public ISagaBroker Get(IServiceProvider serviceProvider,
        SagaType sagaType, object? implSaga, ListenerType listenerType)
    {
        var brokerImplType = typeof(SagaBroker<,>).MakeGenericType(
            sagaType.SagaDataType, listenerType.MessageType.Type);

        var context = new MessagingContextInternal(heavyIO)
        {
            CorrelationId = ((implSaga as ISaga)!).CorrelationId
        };

        return (ISagaBroker)ActivatorUtilities.CreateInstance(serviceProvider,
            brokerImplType, implSaga, context);
    }

}