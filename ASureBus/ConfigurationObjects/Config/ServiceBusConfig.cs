using ASureBus.Abstractions.Configurations;

namespace ASureBus.ConfigurationObjects.Config;

public sealed class ServiceBusConfig : IConfigureAzureServiceBus
{
    public required string ConnectionString { get; set; }
    public string? TransportType { get; set; }
    public int? MaxRetries { get; set; }
    public int? DelayInSeconds { get; set; }
    public int? MaxDelayInSeconds { get; set; }
    public int? TryTimeoutInSeconds { get; set; }
    public string? ServiceBusRetryMode { get; set; }
    public int? MaxConcurrentCalls { get; set; }
    public bool? EnableMessageLockAutoRenewal { get; set; }
    public int? MessageLockRenewalPreemptiveThresholdInSeconds { get; set; }
}