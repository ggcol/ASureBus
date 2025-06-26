using ASureBus.Core.Entities;
using ASureBus.Services.ServiceBus;
using ASureBus.Utils;
using Azure.Messaging.ServiceBus;

namespace ASureBus.Core.Messaging;

internal sealed class MessageEmitter(IAzureServiceBusService serviceBusService)
    : IMessageEmitter
{
    public async Task FlushAll(ICollectMessage collector,
        CancellationToken cancellationToken = default)
    {
        while (collector.Messages.Count != 0)
        {
            var asbMessage = collector.Messages.First();

            var destination = asbMessage.Header.IsCommand
                ? await serviceBusService
                    .ConfigureQueue(asbMessage.Header.Destination, cancellationToken)
                    .ConfigureAwait(false)
                : await serviceBusService
                    .ConfigureTopicForSender(asbMessage.Header.Destination, cancellationToken)
                    .ConfigureAwait(false);

            if (asbMessage.Header.IsScheduled)
            {
                await EmitScheduled(asbMessage, destination, asbMessage.Header.ScheduledTime!.Value,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                await Emit(asbMessage, destination, cancellationToken)
                    .ConfigureAwait(false);
            }

            collector.Messages.Dequeue();
        }
    }

    private async Task Emit(IAsbMessage message, string destination,
        CancellationToken cancellationToken)
    {
        var sender = serviceBusService.GetSender(destination);
        
        var sbm = ToSdkMessage(message);

        await sender.SendMessageAsync(sbm, cancellationToken)
            .ConfigureAwait(false);
    }
    
    private async Task EmitScheduled(IAsbMessage message, string destination,
        DateTimeOffset delay, CancellationToken cancellationToken)
    {
        var sender = serviceBusService.GetSender(destination);
        
        var sbm = ToSdkMessage(message);

        await sender.ScheduleMessageAsync(sbm, delay, cancellationToken)
            .ConfigureAwait(false);
    }

    private static ServiceBusMessage ToSdkMessage(object message)
    {
        var serialized = Serializer.Serialize(message);
        return new ServiceBusMessage(serialized);
    }
}