using ASureBus.Accessories.Heavy;
using ASureBus.Core.Entities;
using ASureBus.Utils;

namespace ASureBus.Core.MessageProcessing;

internal abstract class MessageProcessor
{
    protected async Task<AsbMessageHeader?> GetMessageHeader(BinaryData messageBody, 
        CancellationToken cancellationToken)
    {
        var des = await Serializer
            .Deserialize<DeserializeAsbMessageHeader>(messageBody.ToStream(), cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return des?.Header;
    }

    protected bool UsesHeavies(IAsbMessage asbMessage)
    {
        return HeavyIo.IsHeavyConfigured()
               && asbMessage.Header.Heavies is not null
               && asbMessage.Header.Heavies.Any();
    }

    protected static async Task DeleteHeavies(IAsbMessage asbMessage,
        CancellationToken cancellationToken)
    {
        foreach (var heavyRef in asbMessage.Header.Heavies!)
        {
            await HeavyIo.Delete(asbMessage.Header.MessageId, heavyRef, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}