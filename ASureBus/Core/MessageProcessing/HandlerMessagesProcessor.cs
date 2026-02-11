using System.Runtime.Serialization;
using ASureBus.Abstractions;
using ASureBus.Core.Enablers;
using ASureBus.Core.Messaging;
using ASureBus.Core.TypesHandling.Entities;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace ASureBus.Core.MessageProcessing;

internal sealed class HandlerMessagesProcessor(
    IServiceProvider serviceProvider,
    IMessageEmitter messageEmitter,
    ILogger<HandlerMessagesProcessor> logger,
    IBrokerFactory brokerFactory)
    : MessageProcessor(logger), IProcessHandlerMessages
{
    public async Task ProcessMessage(HandlerType handlerType,
        ProcessMessageEventArgs args)
    {
        var messageHeader = await GetMessageHeader(args.Message.Body, args.CancellationToken).ConfigureAwait(false);
        var correlationId = messageHeader.CorrelationId;

        try
        {
            var broker = brokerFactory.Get(serviceProvider, handlerType, correlationId);

            var asbMessage = await broker
                .Handle(args.Message.Body, args.CancellationToken)
                .ConfigureAwait(false);

            await args
                .CompleteMessageAsync(args.Message, args.CancellationToken)
                .ConfigureAwait(false);

            await messageEmitter.FlushAll(broker.Collector, args.CancellationToken)
                .ConfigureAwait(false);
        }
        catch (ServiceBusException sbEx)
        {
            logger.LogCritical(
                sbEx,
                "Message {MessageId} of type {MessageType} is going to be lost",
                args.Message.MessageId,
                handlerType.MessageType.Type.Name);
        }
        catch (Exception ex)
            when (ex is SerializationException
                      or ArgumentException)
        {
            await args.DeadLetterMessageAsync(
                    args.Message,
                    ex.GetType().Name,
                    ex.Message,
                    args.CancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
            when (ex is IFailFast)
        {
            var failFastMsg = "has failed and will be dead-lettered";
            await DeadLetterMessage(args, ex, correlationId, handlerType.MessageType.Type, failFastMsg)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (!IsMaxDeliveryCountExceeded(args.Message.DeliveryCount)) throw;

            const string maxRetriesMsg = "has reached max retries configured and will be dead-lettered";

            await DeadLetterMessage(args, ex, correlationId, handlerType.MessageType.Type, maxRetriesMsg)
                .ConfigureAwait(false);
        }
    }

    public async Task ProcessError(HandlerType handlerType,
        ProcessErrorEventArgs args)
    {
        var broker = brokerFactory.Get(serviceProvider, handlerType);

        await broker.HandleError(args.Exception, args.CancellationToken)
            .ConfigureAwait(false);
    }
}