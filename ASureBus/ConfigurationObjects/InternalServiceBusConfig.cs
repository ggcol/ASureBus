using ASureBus.Abstractions.Configurations;
using Azure.Messaging.ServiceBus;

namespace ASureBus.ConfigurationObjects;

internal sealed class InternalServiceBusConfig
{
    public string ConnectionString { get; }
    public ServiceBusClientOptions ClientOptions { get; set; }

    public InternalServiceBusConfig(IConfigureAzureServiceBus config)
    {
        ConnectionString = config.ConnectionString;
        ClientOptions = new ServiceBusClientOptions
        {
            TransportType =
                Enum.TryParse<ServiceBusTransportType>(config.TransportType, out var transportType)
                    ? transportType
                    : Defaults.ServiceBus.CLIENT_OPTIONS.TransportType,

            RetryOptions = new ServiceBusRetryOptions
            {
                Mode = Enum.TryParse<ServiceBusRetryMode>(config.ServiceBusRetryMode,
                    out var retryMode)
                    ? retryMode
                    : Defaults.ServiceBus.CLIENT_OPTIONS.RetryOptions.Mode,

                MaxRetries = config.MaxRetries
                             ?? Defaults.ServiceBus.CLIENT_OPTIONS.RetryOptions.MaxRetries,

                Delay = config.DelayInSeconds is not null
                    ? TimeSpan.FromSeconds(config.DelayInSeconds.Value)
                    : Defaults.ServiceBus.CLIENT_OPTIONS.RetryOptions.Delay,

                MaxDelay = config.MaxDelayInSeconds is not null
                    ? TimeSpan.FromSeconds(config.MaxDelayInSeconds.Value)
                    : Defaults.ServiceBus.CLIENT_OPTIONS.RetryOptions.MaxDelay,

                TryTimeout = config.TryTimeoutInSeconds is not null
                    ? TimeSpan.FromSeconds(config.TryTimeoutInSeconds.Value)
                    : Defaults.ServiceBus.CLIENT_OPTIONS.RetryOptions.TryTimeout
            }
        };
    }
}