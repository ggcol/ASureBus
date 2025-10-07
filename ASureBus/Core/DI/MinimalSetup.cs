// Copyright (c) 2025 Gianluca Colombo (red.Co)
//
// This file is part of ASureBus (https://github.com/ggcol/ASureBus).
//
// ASureBus is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation, either version 3 of
// the License, or (at your option) any later version.
//
// ASureBus is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with ASureBus. If not, see <https://www.gnu.org/licenses/>.

using ASureBus.Abstractions;
using ASureBus.Abstractions.Configurations;
using ASureBus.ConfigurationObjects;
using ASureBus.ConfigurationObjects.Config;
using ASureBus.ConfigurationObjects.Exceptions;
using ASureBus.ConfigurationObjects.Options;
using ASureBus.Core.Caching;
using ASureBus.Core.MessageProcessing;
using ASureBus.Core.MessageProcessing.LockHandling;
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

    public static IHostBuilder UseAsb(this IHostBuilder hostBuilder, Action<ServiceBusOptions> options)
    {
        LoadSettings(options);
        return UseAsb(hostBuilder);
    }
    
    public static IHostBuilder UseSendOnlyAsb(this IHostBuilder hostBuilder, Action<ServiceBusOptions> options)
    {
        LoadSettings(options);
        return UseAsb(hostBuilder, true);
    }

    private static void LoadSettings<TSettings>(IHostBuilder hostBuilder)
        where TSettings : class, IConfigureAzureServiceBus, new()
    {
        hostBuilder.ConfigureServices((hostBuilderContext, _) =>
        {
            var settings = ConfigProvider.LoadSettings<TSettings>(hostBuilderContext.Configuration);

            var internalConfig = new InternalServiceBusConfig(settings);
            AsbConfiguration.ServiceBus = internalConfig;
        });
    }

    private static void LoadSettings(ServiceBusConfig? serviceBusConfig)
    {
        if (serviceBusConfig is null)
            throw new ConfigurationNullException(nameof(ServiceBusConfig));

        var internalConfig = new InternalServiceBusConfig(serviceBusConfig);
        AsbConfiguration.ServiceBus = internalConfig;
    }

    private static void LoadSettings(Action<ServiceBusOptions> options)
    {
        var opt = new ServiceBusOptions();
        options(opt);
        
        if (string.IsNullOrWhiteSpace(opt.ConnectionString))
            throw new ConfigurationNullException(nameof(ServiceBusOptions.ConnectionString));

        var internalConfig = new InternalServiceBusConfig(opt);
        AsbConfiguration.ServiceBus = internalConfig;
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
                    .AddSingleton<ISagaFactory, SagaFactory>()
                    .AddSingleton<IMessagingContext, MessagingContext>()
                    .AddSingleton<IMessageEmitter, MessageEmitter>()
                    .AddSingleton<ISagaIO, SagaIO>()
                    .AddSingleton<IProcessSagaMessages, SagaMessagesProcessor>()
                    .AddSingleton<IProcessHandlerMessages, HandlerMessagesProcessor>()
                    .AddSingleton<IMessageLockObserver, MessageLockObserver>();
                

                if (!isSendOnly)
                {
                    services.AddHostedService<AsbWorker>();
                }
            });
    }
}