using ASureBus.Core.TypesHandling.Entities;
using Azure.Messaging.ServiceBus;

namespace ASureBus.Core.MessageProcessing;

internal interface IProcessSagaMessages
{
    internal Task ProcessMessage(SagaType sagaType,
        SagaHandlerType listenerType, ProcessMessageEventArgs args);

    internal Task ProcessError(SagaType sagaType, SagaHandlerType listenerType,
        ProcessErrorEventArgs args);
}