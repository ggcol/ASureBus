using ASureBus.Abstractions;
using ASureBus.Accessories.Heavy;
using ASureBus.Core.Entities;
using ASureBus.Core.Messaging;
using ASureBus.Utils;

namespace ASureBus.Core.Enablers;

internal abstract class BrokerBehavior<TMessage>(
    IMessagingContext context)
    where TMessage : IAmAMessage
{
    protected readonly IMessagingContext Context = context;
    
    public ICollectMessage Collector => (ICollectMessage)Context;

    protected async Task<AsbMessage<TMessage>?> GetFrom(BinaryData binaryData,
        CancellationToken cancellationToken = default)
    {
        var asbMessage = await Serializer
            .Deserialize<AsbMessage<TMessage>?>(binaryData.ToStream(),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (!HeavyIo.IsHeavyConfigured()) return asbMessage;
        
        if (asbMessage.Heavies is not null && asbMessage.Heavies.Any())
        {
            await HeavyIo.Load(asbMessage.Message, asbMessage.Heavies,
                    asbMessage.MessageId, cancellationToken)
                .ConfigureAwait(false);
        }

        return asbMessage;
    }
}