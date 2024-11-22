﻿using Azure.Messaging.ServiceBus;

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
    }
    
    internal static class SqlServerSagaPersistence
    {
        internal const string SCHEMA = "sagas";
    }
}