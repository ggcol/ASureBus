using ASureBus.Abstractions;
using ASureBus.Core.Caching;
using ASureBus.Core.Enablers;
using ASureBus.Core.Entities.NotNullPatternReturns;
using ASureBus.Core.Exceptions;
using ASureBus.Core.Messaging;
using ASureBus.Core.Sagas;
using ASureBus.Core.TypesHandling.Entities;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace ASureBus.Core.MessageProcessing;

internal sealed class SagaMessagesProcessor(
    ISagaFactory sagaFactory,
    ILogger<SagaMessagesProcessor> logger,
    IServiceProvider serviceProvider,
    ISagaIO sagaIo,
    IAsbCache cache,
    IMessageEmitter messageEmitter)
    : MessageProcessor, IProcessSagaMessages
{
    public async Task ProcessMessage(SagaType sagaType,
        SagaHandlerType listenerType, ProcessMessageEventArgs args)
    {
        var messageHeader = await GetMessageHeader(args.Message.Body, args.CancellationToken).ConfigureAwait(false);
        var correlationId = messageHeader.CorrelationId;

        try
        {
            var saga = await sagaFactory.Get(sagaType, listenerType, correlationId, args.CancellationToken)
                .ConfigureAwait(false);

            if (saga is SagaAlreadyCompleted)
            {
                logger.LogInformation(
                    "Handling timeout for saga {SagaType} with correlation id {CorrelationId}, saga already completed, skipping",
                    sagaType.Type.Name, correlationId);

                await args.CompleteMessageAsync(args.Message, args.CancellationToken)
                    .ConfigureAwait(false);

                return;
            }

            var broker = BrokerFactory.Get(serviceProvider, sagaType, saga, listenerType);

            var asbMessage = await broker.Handle(args.Message.Body, args.CancellationToken)
                .ConfigureAwait(false);

            if (UsesHeavies(asbMessage))
            {
                await DeleteHeavies(asbMessage, args.CancellationToken).ConfigureAwait(false);
            }

            await args.CompleteMessageAsync(args.Message, args.CancellationToken)
                .ConfigureAwait(false);

            if (!IsComplete(saga!))
            {
                await sagaIo.Unload(saga, correlationId, sagaType).ConfigureAwait(false);

                cache.Upsert(correlationId, saga);
            }

            await messageEmitter.FlushAll(broker.Collector, args.CancellationToken)
                .ConfigureAwait(false);
        }
        catch (ServiceBusException sbEx)
        {
            logger.LogCritical(
                sbEx,
                "Message {MessageId} of type {MessageType} is going to be lost",
                args.Message.MessageId,
                listenerType.MessageType.Type.Name);
        }
        catch (Exception ex)
        {
            /*
             * exception caught here is always TargetInvocationException
             * since every saga's handle method is called by reflection
             * the actual exception should be stored in InnerException
             */

            if (ex.InnerException is AsbException) throw;
            throw new AsbException
            {
                OriginalException = ex.InnerException ?? ex,
                CorrelationId = correlationId
            };
        }
    }

    public async Task ProcessError(SagaType sagaType, SagaHandlerType listenerType,
        ProcessErrorEventArgs args)
    {
        var ex = args.Exception as AsbException;
        var correlationId = ex!.CorrelationId;

        var saga = await sagaFactory.Get(sagaType, listenerType, correlationId, args.CancellationToken)
            .ConfigureAwait(false);

        if (saga is SagaAlreadyCompleted)
        {
            logger.LogInformation(
                "Handling timeout for saga {SagaType} with correlation id {CorrelationId}, saga already completed, skipping",
                sagaType.Type.Name, correlationId);

            return;
        }

        var broker = BrokerFactory.Get(serviceProvider, sagaType, saga, listenerType);

        await broker.HandleError(ex?.OriginalException!, args.CancellationToken)
            .ConfigureAwait(false);
    }

    private static bool IsComplete(object implSaga)
    {
        return ((ISaga)implSaga).IsComplete;
    }
}