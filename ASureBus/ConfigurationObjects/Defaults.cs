using Azure.Messaging.ServiceBus;

namespace ASureBus.ConfigurationObjects;

// ReSharper disable InconsistentNaming
internal static class Defaults
{
    internal static class Cache
    {
        internal static readonly TimeSpan EXPIRATION = TimeSpan.FromMinutes(5);
        internal const string TOPIC_CONFIG_PREFIX = "topicConfig";
        internal const string SERVICE_BUS_SENDER_PREFIX = "sender";
    }

    internal static class ServiceBus
    {
        internal static readonly ServiceBusClientOptions CLIENT_OPTIONS = new()
        {
            TransportType = ServiceBusTransportType.AmqpWebSockets,
            RetryOptions = new ServiceBusRetryOptions
            {
                Mode = ServiceBusRetryMode.Fixed,
                MaxRetries = 3,
                Delay = TimeSpan.FromSeconds(0.8),
                MaxDelay = TimeSpan.FromSeconds(60),
                TryTimeout = TimeSpan.FromSeconds(300)
            }
        };
        
        internal const int MAX_CONCURRENT_CALLS = 20;
        internal const bool ENABLE_MESSAGE_LOCK_AUTO_RENEWAL = false;
        internal const int MESSAGE_LOCK_RENEWAL_PREEMPTIVE_THRESHOLD_IN_SECONDS = 10;
        internal static TimeSpan MAX_AUTO_LOCK_RENEWAL_DURATION = TimeSpan.FromMinutes(5); //this is Azure Service Bus default
    }
    
    internal static class SqlServerSagaPersistence
    {
        internal const string SCHEMA = "sagas";
    }
}