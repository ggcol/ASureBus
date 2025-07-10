using ASureBus.Abstractions;
using ASureBus.Core.Messaging;
using ASureBus.Core.TypesHandling.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ASureBus.Core.Enablers;

internal static class BrokerFactory
{
    internal static IHandlerBroker Get(IServiceProvider serviceProvider,
        HandlerType handlerType, Guid? correlationId = null)
    {
        var implListener = ActivatorUtilities.CreateInstance(
            serviceProvider, handlerType.Type);

        var brokerImplType = typeof(HandlerBroker<>)
            .MakeGenericType(handlerType.MessageType.Type);

        var context = new MessagingContextInternal();

        if (correlationId is not null)
        {
            context.CorrelationId = correlationId.Value;
        }

        return (IHandlerBroker)ActivatorUtilities.CreateInstance(
            serviceProvider, brokerImplType, implListener, context);
    }

    internal static ISagaBroker Get(IServiceProvider serviceProvider,
        SagaType sagaType, object? implSaga, ListenerType listenerType)
    {
        var brokerImplType = typeof(SagaBroker<,>).MakeGenericType(
            sagaType.SagaDataType, listenerType.MessageType.Type);

        var context = new MessagingContextInternal
        {
            CorrelationId = ((implSaga as ISaga)!).CorrelationId
        };

        return (ISagaBroker)ActivatorUtilities.CreateInstance(serviceProvider,
            brokerImplType, implSaga, context);
    }

}