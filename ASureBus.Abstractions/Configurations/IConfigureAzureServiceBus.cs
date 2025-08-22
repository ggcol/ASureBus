namespace ASureBus.Abstractions.Configurations;

public interface IConfigureAzureServiceBus
{
    public string ConnectionString { get; set; }
    /// <summary>
    /// May be "AmqpTcp" or "AmqpWebSockets", default is "AmqpWebSocket".
    /// Maps to Azure.Messaging.ServiceBus.ServiceBusTransportType.
    /// </summary>
    public string? TransportType { get; set; }
    public int? MaxRetries { get; set; }
    public int? DelayInSeconds { get; set; }
    public int? MaxDelayInSeconds { get; set; }
    public int? TryTimeoutInSeconds { get; set; }
    /// <summary>
    /// May be "fixed" or "exponential", default is "fixed".
    /// Maps to Azure.Messaging.ServiceBus.ServiceBusRetryMode.
    /// </summary>
    public string? ServiceBusRetryMode { get; set; }
    public int? MaxConcurrentCalls { get; set; } 
    public bool? EnableMessageLockAutoRenewal { get; set; }
    public int? MessageLockRenewalPreemptiveThresholdInSeconds { get; set; }
}