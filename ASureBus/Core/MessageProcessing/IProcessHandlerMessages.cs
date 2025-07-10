using ASureBus.Core.TypesHandling.Entities;
using Azure.Messaging.ServiceBus;

namespace ASureBus.Core.MessageProcessing;

internal interface IProcessHandlerMessages
{
    internal Task ProcessMessage(HandlerType handlerType, ProcessMessageEventArgs args);

    internal Task ProcessError(HandlerType handlerType, ProcessErrorEventArgs args);
}