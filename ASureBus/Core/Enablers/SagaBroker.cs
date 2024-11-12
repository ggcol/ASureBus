﻿using ASureBus.Abstractions;
using ASureBus.Core.Entities;

namespace ASureBus.Core.Enablers;

internal sealed class SagaBroker<TSagaData, TMessage>(
    Saga<TSagaData> saga,
    IMessagingContext context)
    : BrokerBehavior<TMessage>(context), ISagaBroker
    where TSagaData : SagaData, new()
    where TMessage : IAmAMessage
{
    public async Task<IAsbMessage> Handle(BinaryData binaryData,
        CancellationToken cancellationToken = default)
    {
        var method = saga
            .GetType()
            .GetMethods()
            /*
             * TODO this nameof() is misleading, it doesn't refers directly to
             * Saga<>.Handle
             */
            .FirstOrDefault(method =>
                method.Name.Equals(nameof(Handle)) &&
                method.GetParameters().Length == 3 &&
                method.GetParameters()[0].ParameterType == typeof(TMessage)
            );

        var asbMessage = await GetFrom(binaryData, cancellationToken)
            .ConfigureAwait(false);

        if (method is not null)
        {
            await (Task)method.Invoke(saga,
            [
                asbMessage.Message, Context, cancellationToken
            ]);
        }

        return asbMessage;
    }

    public async Task HandleError(Exception ex,
        CancellationToken cancellationToken = default)
    {
        var method = saga
            .GetType()
            .GetMethods()
            /*
             * TODO this nameof() is misleading, it doesn't refers directly to
             * Saga<>.Handle
             */
            .FirstOrDefault(method =>
                method.Name.Equals(nameof(HandleError)) &&
                method.GetParameters().Length == 3 &&
                method.GetParameters()[0].ParameterType == typeof(Exception)
            );

        if (method is not null)
        {
            await (Task)method.Invoke(saga,
            [
                ex, Context, cancellationToken
            ]);
        }
    }
}