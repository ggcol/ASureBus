using ASureBus.Abstractions;
using ASureBus.Abstractions.Configurations;
using ASureBus.ConfigurationObjects;
using ASureBus.ConfigurationObjects.Exceptions;
using ASureBus.Core.Caching;
using ASureBus.Core.Messaging;
using ASureBus.Core.Sagas;
using ASureBus.Core.TypesHandling;
using ASureBus.Services.ServiceBus;
using ASureBus.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ASureBus.Core.DI;

public static class MinimalSetup
{
    public static IHostBuilder UseAsb<TSettings>(this IHostBuilder hostBuilder)
        where TSettings : class, IConfigureAzureServiceBus, new()
    {
        LoadSettings<TSettings>(hostBuilder);
        return UseAsb(hostBuilder);
    }

    public static IHostBuilder UseSendOnlyAsb<TSettings>(this IHostBuilder hostBuilder)
        where TSettings : class, IConfigureAzureServiceBus, new()
    {
        LoadSettings<TSettings>(hostBuilder);
        return UseAsb(hostBuilder, true);
    }

    public static IHostBuilder UseAsb(this IHostBuilder hostBuilder,
        ServiceBusConfig? serviceBusConfig)
    {
        LoadSettings(serviceBusConfig);
        return UseAsb(hostBuilder);
    }

    public static IHostBuilder UseSendOnlyAsb(this IHostBuilder hostBuilder,
        ServiceBusConfig? serviceBusConfig)
    {
        LoadSettings(serviceBusConfig);
        return UseAsb(hostBuilder, true);
    }

    private static void LoadSettings<TSettings>(IHostBuilder hostBuilder)
        where TSettings : class, IConfigureAzureServiceBus, new()
    {
        hostBuilder.ConfigureServices((hostBuilderContext, _) =>
        {
            var settings = ConfigProvider.LoadSettings<TSettings>(hostBuilderContext.Configuration);

            AsbConfiguration.ServiceBus = new InternalServiceBusConfig(settings);
        });
    }

    private static void LoadSettings(ServiceBusConfig? serviceBusConfig)
    {
        if (serviceBusConfig is null)
            throw new ConfigurationNullException(nameof(ServiceBusConfig));

        AsbConfiguration.ServiceBus = new InternalServiceBusConfig(serviceBusConfig);
    }

    private static IHostBuilder UseAsb(IHostBuilder hostBuilder, bool isSendOnly = false)
    {
        return hostBuilder
            .ConfigureServices((_, services) =>
            {
                /*
                 * TODO think of this:
                 * what's registered here is broad-wide available in the application...
                 */
                services
                    .AddSingleton<IAsbCache, AsbCache>()
                    .AddSingleton<ITypesLoader, TypesLoader>()
                    .AddSingleton<IAzureServiceBusService, AzureServiceBusService>()
                    .AddSingleton<ISagaBehaviour, SagaBehaviour>()
                    .AddSingleton<IMessagingContext, MessagingContext>()
                    .AddSingleton<IMessageEmitter, MessageEmitter>()
                    .AddSingleton<ISagaIO, SagaIO>();
                

                if (!isSendOnly)
                {
                    services.AddHostedService<AsbWorker>();
                }
            });
    }
}