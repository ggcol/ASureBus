using ASureBus.Abstractions;
using ASureBus.Core.Entities;
using ASureBus.Core.Messaging;
using ASureBus.IO.Heavies;
using ASureBus.Utils;

namespace ASureBus.Core.Enablers;

internal abstract class BrokerBehavior<TMessage>(
    IMessagingContext context,
    IHeavyIO heavyIO)
    where TMessage : IAmAMessage
{
    protected readonly IMessagingContext Context = context;

    public ICollectMessage Collector => (ICollectMessage)Context;

    protected async Task<AsbMessage<TMessage>?> GetFrom(BinaryData binaryData,
        CancellationToken cancellationToken = default)
    {
        var asbMessage = await Serializer
            .Deserialize<AsbMessage<TMessage>?>(binaryData.ToStream(), cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (!heavyIO.IsHeavyConfigured) return asbMessage;

        if (asbMessage?.Header.Heavies is not null && asbMessage.Header.Heavies.Any())
        {
            await heavyIO.Load(
                    asbMessage.Message,
                    asbMessage.Header.Heavies,
                    asbMessage.Header.MessageId,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        return asbMessage;
    }
}