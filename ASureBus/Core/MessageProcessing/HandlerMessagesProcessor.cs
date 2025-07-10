using System.Runtime.Serialization;
using ASureBus.Core.Enablers;
using ASureBus.Core.Messaging;
using ASureBus.Core.Sagas;
using ASureBus.Core.TypesHandling.Entities;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace ASureBus.Core.MessageProcessing;

internal sealed class HandlerMessagesProcessor(
    IServiceProvider serviceProvider,
    IMessageEmitter messageEmitter,
    ILogger<HandlerMessagesProcessor> logger,
    ISagaFactory sagaFactory)
    : MessageProcessor, IProcessHandlerMessages
{
    public async Task ProcessMessage(HandlerType handlerType,
        ProcessMessageEventArgs args)
    {
        var messageHeader = await GetMessageHeader(args.Message.Body, args.CancellationToken).ConfigureAwait(false);
        var correlationId = messageHeader.CorrelationId;

        try
        {
            var broker = BrokerFactory.Get(serviceProvider, handlerType, correlationId);

            var asbMessage = await broker
                .Handle(args.Message.Body, args.CancellationToken)
                .ConfigureAwait(false);

            if (UsesHeavies(asbMessage))
            {
                await DeleteHeavies(asbMessage, args.CancellationToken).ConfigureAwait(false);
            }

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
        {
            //TODO implement FailFast mechanism

            var actualRetries = args.Message.DeliveryCount;
            var maxRetries = AsbConfiguration.ServiceBus.ClientOptions.RetryOptions.MaxRetries;
            
            if (actualRetries < maxRetries) throw;
            
            logger.LogError(
                ex,
                "Message {MessageId} with CorrelationId: {CorrealationId} of type {MessageType} has reached max retries ({MaxRetries}) and will be dead-lettered",
                args.Message.MessageId,
                correlationId,
                handlerType.MessageType.Type.Name,
                maxRetries);
            
            var reasonDescription = string.Join('\n',
                $"Message: {ex.Message}",
                $"Stack Trace: {ex.StackTrace}",
                $"CorrelationId: {correlationId}"
            );

            await args.DeadLetterMessageAsync(
                    args.Message,
                    ex.GetType().Name,
                    ex.Message,
                    args.CancellationToken)
                .ConfigureAwait(false);
        }
    }

    public async Task ProcessError(HandlerType handlerType,
        ProcessErrorEventArgs args)
    {
        var broker = BrokerFactory.Get(serviceProvider, handlerType);

        await broker.HandleError(args.Exception, args.CancellationToken)
            .ConfigureAwait(false);
    }
}