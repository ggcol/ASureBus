using ASureBus.ConfigurationObjects.Options;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;

namespace ASureBus.Core.DI;

public static class Configurations
{
    public static IHostBuilder ConfigureMessageLockHandling(
        this IHostBuilder hostBuilder, 
        Action<MessageLockRenewalOptions> enableMessageLockAutoRenewal)
    {
        var opt = new MessageLockRenewalOptions();
        enableMessageLockAutoRenewal(opt);

        AsbConfiguration.MessageLockOptions = opt;
        return hostBuilder;
    }
    
    public static IHostBuilder ConfigureMaxConcurrentCalls(
        this IHostBuilder hostBuilder, 
        Action<MaxConcurrentCallsOptions> maxConcurrentCalls)
    {
        var opt = new MaxConcurrentCallsOptions();
        maxConcurrentCalls(opt);

        AsbConfiguration.MaxConcurrentCalls = opt.MaxConcurrentCalls;
        return hostBuilder;
    }
    
    public static IHostBuilder ConfigureServiceBusClientOptions(
        this IHostBuilder hostBuilder, 
        Action<ServiceBusClientOptions> serviceBusClientOptions)
    {
        var opt = new ServiceBusClientOptions();
        serviceBusClientOptions(opt);

        AsbConfiguration.ServiceBus.ClientOptions = opt;
        return hostBuilder;
    }
}