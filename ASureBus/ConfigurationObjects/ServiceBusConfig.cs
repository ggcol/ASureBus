using ASureBus.Abstractions.Configurations;

namespace ASureBus.ConfigurationObjects;

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
}

public sealed class ServiceBusOptions: IConfigureAzureServiceBus
{
    public string? ConnectionString { get; set; }
    public string? TransportType { get; set; }
    public int? MaxRetries { get; set; }
    public int? DelayInSeconds { get; set; }
    public int? MaxDelayInSeconds { get; set; }
    public int? TryTimeoutInSeconds { get; set; }
    public string? ServiceBusRetryMode { get; set; }
    public int? MaxConcurrentCalls { get; set; }
}