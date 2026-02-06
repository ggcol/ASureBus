using ASureBus.Core.TypesHandling.Entities;
using Azure.Messaging.ServiceBus;

namespace ASureBus.IO.ServiceBus;

internal interface IAzureServiceBusService
{
    internal Task<ServiceBusProcessor> GetProcessor(
        ListenerType handler, CancellationToken cancellationToken = default);

    internal Task<string> ConfigureQueue(string queueName,
        CancellationToken cancellationToken = default);

    internal Task<string> ConfigureTopicForSender(string topicName,
        CancellationToken cancellationToken = default);

    internal ServiceBusSender GetSender(string destination);
}