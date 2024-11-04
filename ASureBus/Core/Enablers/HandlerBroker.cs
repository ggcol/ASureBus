﻿using ASureBus.Abstractions;
using ASureBus.Accessories.Heavy;

namespace ASureBus.Core.Enablers;

internal sealed class HandlerBroker<TMessage>(
    IHandleMessage<TMessage> handler, IMessagingContext context)
    : BrokerBehavior<TMessage>(context), IHandlerBroker<TMessage>
    where TMessage : IAmAMessage
{
    public async Task Handle(BinaryData binaryData,
        CancellationToken cancellationToken = default)
    {
        var rsbMessage = await GetFrom(binaryData, cancellationToken);

        await handler.Handle(rsbMessage.Message, _context, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task HandleError(Exception ex,
        CancellationToken cancellationToken = default)
    {
        await handler.HandleError(ex, _context, cancellationToken)
            .ConfigureAwait(false);
    }
}