using System.Reflection;
using ASureBus.Core.Caching;
using ASureBus.Core.TypesHandling;
using ASureBus.Core.TypesHandling.Entities;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace ASureBus.IO.ServiceBus;

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

        return AsbConfiguration.UseConsumerScopedQueueForTopics
            ? _sbClient.CreateProcessor(topicConfig.QueueName, _processorOptions)
            : _sbClient.CreateProcessor(topicConfig.Name, topicConfig.SubscriptionName, _processorOptions);
    }

    public async Task<string> ConfigureQueue(string queueName,
        CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(queueName, out string queue)) return queue;

        var admClient = MakeAdmClient();

        await EnsureEntity(
            () => admClient.QueueExistsAsync(queueName, cancellationToken),
            () => admClient.CreateQueueAsync(queueName, cancellationToken),
            cancellationToken).ConfigureAwait(false);

        return cache.Set(queueName, queueName, AsbConfiguration.Cache.Expiration)!;
    }

    public async Task<string> ConfigureTopicForSender(string topicName,
        CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(topicName, out string topic)) return topic;

        var admClient = MakeAdmClient();

        await EnsureEntity(
            () => admClient.TopicExistsAsync(topicName, cancellationToken),
            () => admClient.CreateTopicAsync(topicName, cancellationToken),
            cancellationToken).ConfigureAwait(false);

        return cache.Set(topicName, topicName, AsbConfiguration.Cache.Expiration)!;
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

        if (cache.TryGetValue(cacheKey, out TopicConfiguration cachedConfig)) return cachedConfig!;

        var admClient = MakeAdmClient();

        await EnsureEntity(
            () => admClient.TopicExistsAsync(config.Name, cancellationToken),
            () => admClient.CreateTopicAsync(config.Name, cancellationToken),
            cancellationToken).ConfigureAwait(false);

        var opt = new CreateSubscriptionOptions(config.Name, config.SubscriptionName);
        
        if (AsbConfiguration.UseConsumerScopedQueueForTopics)
        {
            config.QueueName = QueueName.ForConsumerScopedTopicSubscription(config);

            await ConfigureQueue(config.QueueName, cancellationToken).ConfigureAwait(false);
            opt.ForwardTo = config.QueueName;
        }

        await EnsureEntity(
            () => admClient.SubscriptionExistsAsync(config.Name, config.SubscriptionName, cancellationToken),
            () => admClient.CreateSubscriptionAsync(opt, cancellationToken),
            cancellationToken).ConfigureAwait(false);

        return cache.Set(cacheKey, config, AsbConfiguration.Cache.Expiration)!;
    }

    //TODO store? throwaway?
    private ServiceBusAdministrationClient MakeAdmClient()
    {
        return new ServiceBusAdministrationClient(AsbConfiguration.ServiceBus
            .ConnectionString);
    }

    /*
     * Ensures a Service Bus entity exists, handling concurrent creation and
     * transient 409/40900 conflicts caused by entities in a transitional
     * (e.g. being-deleted) state.
     */
    private static async Task EnsureEntity(
        Func<Task<Azure.Response<bool>>> existsAsync,
        Func<Task> createAsync,
        CancellationToken cancellationToken,
        int maxRetries = 10,
        int delayMs = 3000)
    {
        for (var attempt = 0; attempt <= maxRetries; attempt++)
        {
            var existsResponse = await existsAsync().ConfigureAwait(false);
            if (existsResponse.Value) return;

            try
            {
                await createAsync().ConfigureAwait(false);
                return;
            }
            catch (ServiceBusException ex) when (
                ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
            {
                // Entity exists or is in a transitional state (being deleted).
                // If it truly exists now, we're done. Otherwise retry after a
                // short delay to let the delete complete.
                var recheckResponse = await existsAsync().ConfigureAwait(false);
                if (recheckResponse.Value) return;

                if (attempt == maxRetries) throw;

                await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
            }
        }
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

internal sealed record TopicConfiguration(string Name, string SubscriptionName)
{
    internal string? QueueName { get; set; }
}