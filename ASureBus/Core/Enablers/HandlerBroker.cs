using ASureBus.Abstractions;
using ASureBus.Core.Entities;
using ASureBus.IO.Heavies;

namespace ASureBus.Core.Enablers;

internal sealed class HandlerBroker<TMessage>(
    IHandleMessage<TMessage> handler, 
    IMessagingContext context,
    IHeavyIO heavyIO)
    : BrokerBehavior<TMessage>(context, heavyIO), IHandlerBroker<TMessage>
    where TMessage : IAmAMessage
{
    public async Task<IAsbMessage> Handle(BinaryData binaryData,
        CancellationToken cancellationToken = default)
    {
        var asbMessage = await GetFrom(binaryData, cancellationToken);

        await handler.Handle(asbMessage.Message, Context, cancellationToken)
            .ConfigureAwait(false);
        
        return asbMessage;
    }

    public async Task HandleError(Exception ex,
        CancellationToken cancellationToken = default)
    {
        await handler.HandleError(ex, Context, cancellationToken)
            .ConfigureAwait(false);
    }
}