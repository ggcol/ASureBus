using ASureBus.Accessories.Heavies;
using ASureBus.Core.Entities;
using ASureBus.Utils;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace ASureBus.Core.MessageProcessing;

internal abstract class MessageProcessor(ILogger logger)
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

    protected static bool IsMaxDeliveryCountExceeded(int actualCount)
    {
        return actualCount >= AsbConfiguration.ServiceBus.ClientOptions.RetryOptions.MaxRetries;
    }

    protected Task DeadLetterMessage(ProcessMessageEventArgs args, Exception ex, Guid correlationId, Type messageType,
        string message)
    {
        logger.LogError(
            ex,
            "Message {MessageId} of type {MessageType} with CorrelationId: {CorrelationId} " + message,
            args.Message.MessageId,
            messageType.Name,
            correlationId);

        var reasonDescription = string.Join(Environment.NewLine,
            $"Message: {ex.Message}",
            $"Stack Trace: {ex.StackTrace}",
            $"CorrelationId: {correlationId}"
        );

        return args.DeadLetterMessageAsync(
            args.Message,
            ex.GetType().Name,
            reasonDescription,
            args.CancellationToken);
    }
}