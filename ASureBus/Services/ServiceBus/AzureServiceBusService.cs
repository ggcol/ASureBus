using System.Reflection;
using ASureBus.Core;
using ASureBus.Core.Caching;
using ASureBus.Core.TypesHandling;
using ASureBus.Core.TypesHandling.Entities;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace ASureBus.Services.ServiceBus;

internal sealed class AzureServiceBusService(IAsbCache cache)
    : IAzureServiceBusService
{
    private ServiceBusClient _sbClient { get; } = new(
        AsbConfiguration.ServiceBus.ConnectionString,
        AsbConfiguration.ServiceBus.ClientOptions);
    
    private ServiceBusProcessorOptions _processorOptions { get; } = new()
    {
        MaxConcurrentCalls = AsbConfiguration.MaxConcurrentCalls,
        MaxAutoLockRenewalDuration = AsbConfiguration.MessageLockOptions.MaxAutoLockRenewalDuration,
    };

    public async Task<ServiceBusProcessor> GetProcessor(
        ListenerType handler, CancellationToken cancellationToken = default)
    {
        if (handler.MessageType.IsCommand)
        {
            var queueName = QueueName.Resolve(handler.MessageType.Type);
            
            var queue = await ConfigureQueue(queueName, cancellationToken)
                .ConfigureAwait(false);

            return _sbClient.CreateProcessor(queue, _processorOptions);
        }

        var topicConfig = await ConfigureTopicForReceiver(
                handler.MessageType.Type, cancellationToken)
            .ConfigureAwait(false);

        return _sbClient.CreateProcessor(topicConfig.Name,
            topicConfig.SubscriptionName, _processorOptions);
    }

    public async Task<string> ConfigureQueue(string queueName,
        CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(queueName, out string queue)) return queue;

        var admClient = MakeAdmClient();

        if (!await admClient.QueueExistsAsync(queueName, cancellationToken))
        {
            var rx = await admClient.CreateQueueAsync(
                queueName, cancellationToken);
            queueName = rx.Value.Name;
        }

        return cache.Set(queueName, queueName, AsbConfiguration.Cache.Expiration);
    }

    public async Task<string> ConfigureTopicForSender(string topicName,
        CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(topicName, out string topic)) return topic;

        var admClient = MakeAdmClient();

        if (!await admClient.TopicExistsAsync(topicName, cancellationToken))
        {
            var rx = await admClient.CreateTopicAsync(
                topicName, cancellationToken);
            topicName = rx.Value.Name;
        }

        return cache.Set(topicName, topicName, AsbConfiguration.Cache.Expiration);
    }

    private async Task<TopicConfiguration> ConfigureTopicForReceiver(
        Type messageType, CancellationToken cancellationToken = default)
    {
        var queueName = QueueName.Resolve(messageType);
        
        var config = new TopicConfiguration(
            queueName,
            Assembly.GetEntryAssembly()?.GetName().Name);

        var cacheKey = CacheKey(AsbConfiguration.Cache.TopicConfigPrefix,
            config.Name);

        if (cache.TryGetValue(cacheKey, out TopicConfiguration cachedConfig))
            return cachedConfig;

        var admClient = MakeAdmClient();

        if (!await admClient.TopicExistsAsync(config.Name, cancellationToken))
        {
            _ = await admClient.CreateTopicAsync(
                config.Name, cancellationToken);
        }

        if (!await admClient.SubscriptionExistsAsync(config.Name,
                config.SubscriptionName, cancellationToken))
        {
            _ = await admClient
                .CreateSubscriptionAsync(config.Name, config.SubscriptionName,
                    cancellationToken);
        }

        return cache.Set(cacheKey, config, AsbConfiguration.Cache.Expiration);
    }

    //TODO store? throwaway?
    private ServiceBusAdministrationClient MakeAdmClient()
    {
        return new ServiceBusAdministrationClient(AsbConfiguration.ServiceBus
            .ConnectionString);
    }

    public ServiceBusSender GetSender(string destination)
    {
        var cacheKey = CacheKey(AsbConfiguration.Cache.ServiceBusSenderCachePrefix!, destination);

        return cache.TryGetValue(cacheKey, out ServiceBusSender sender)
            ? sender
            : cache.Set(cacheKey, _sbClient.CreateSender(destination),
                AsbConfiguration.Cache.Expiration);
    }

    private string CacheKey(params string[] values)
    {
        return string.Join("-", values.Where(x => !string.IsNullOrEmpty(x)));
    }
}

public sealed record TopicConfiguration(string Name, string SubscriptionName);