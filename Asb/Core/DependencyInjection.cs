﻿using Asb.Abstractions;
using Asb.Accessories.Heavy;
using Asb.Configurations;
using Asb.Configurations.ConfigObjects;
using Asb.Core.Caching;
using Asb.Core.Messaging;
using Asb.Core.Sagas;
using Asb.Core.TypesHandling;
using Asb.Services;
using Asb.Services.ServiceBus;
using Asb.Services.SqlServer;
using Asb.Services.StorageAccount;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asb.Core;

public static class DependencyInjection
{
    public static IHostBuilder UseRsb<TSettings>(this IHostBuilder hostBuilder)
        where TSettings : class, IConfigureAzureServiceBus, new()
    {
        hostBuilder.ConfigureServices((hostBuilderContext, _) =>
        {
            var settings = ConfigProvider.LoadSettings<TSettings>(hostBuilderContext.Configuration);

            RsbConfiguration.ServiceBus = new ServiceBusConfig
            {
                ServiceBusConnectionString = settings.ServiceBusConnectionString
            };
        });

        return UseRsb(hostBuilder);
    }

    public static IHostBuilder UseRsb(this IHostBuilder hostBuilder,
        ServiceBusConfig? serviceBusConfig)
    {
        if (serviceBusConfig is null)
            throw new ConfigurationNullException(nameof(ServiceBusConfig));

        RsbConfiguration.ServiceBus = serviceBusConfig;

        return UseRsb(hostBuilder);
    }

    private static IHostBuilder UseRsb(IHostBuilder hostBuilder)
    {
        return hostBuilder
            .ConfigureServices((_, services) =>
            {
                /*
                 * TODO think of this:
                 * what's registered here is broad-wide available in the application...
                 */
                services
                    .AddSingleton<IRsbCache, RsbCache>()
                    .AddSingleton<IRsbTypesLoader, RsbTypesLoader>()
                    .AddSingleton<IAzureServiceBusService, AzureServiceBusService>()
                    .AddSingleton<ISagaBehaviour, SagaBehaviour>()
                    .AddSingleton<IMessagingContext, MessagingContext>()
                    .AddSingleton<IMessageEmitter, MessageEmitter>();

                services.AddHostedService<RsbWorker>();
            });
    }

    public static IHostBuilder UseHeavyProps<TSettings>(
        this IHostBuilder hostBuilder)
        where TSettings : class, IConfigureHeavyProperties, new()
    {
        hostBuilder.ConfigureServices((hostBuilderContext, _) =>
        {
            var settings = ConfigProvider.LoadSettings<TSettings>(hostBuilderContext.Configuration);

            RsbConfiguration.HeavyProps = new HeavyPropertiesConfig
            {
                DataStorageConnectionString = settings.DataStorageConnectionString,
                DataStorageContainer = settings.DataStorageContainer
            };
        });

        return UseHeavyProps(hostBuilder);
    }

    public static IHostBuilder UseHeavyProps(
        this IHostBuilder hostBuilder, HeavyPropertiesConfig? heavyPropsConfig)
    {
        if (heavyPropsConfig is null)
            throw new ConfigurationNullException(nameof(HeavyPropertiesConfig));

        RsbConfiguration.HeavyProps = heavyPropsConfig;

        return UseHeavyProps(hostBuilder);
    }

    private static IHostBuilder UseHeavyProps(IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices((_, services) =>
        {
            services.AddScoped<IHeavyIO, HeavyIO>();
        });
    }

    public static IHostBuilder ConfigureRsbCache<TSettings>(
        this IHostBuilder hostBuilder)
        where TSettings : class, IConfigureRsbCache, new()
    {
        return hostBuilder.ConfigureServices((hostBuilderContext, _) =>
        {
            var settings = ConfigProvider.LoadSettings<TSettings>(hostBuilderContext.Configuration);

            RsbConfiguration.Cache = new RsbCacheConfig
            {
                Expiration = settings.Expiration,
                TopicConfigPrefix = settings.TopicConfigPrefix,
                ServiceBusSenderCachePrefix = settings.ServiceBusSenderCachePrefix
            };
        });
    }

    public static IHostBuilder ConfigureRsbCache(this IHostBuilder hostBuilder,
        RsbCacheConfig rsbCacheConfig)
    {
        if (rsbCacheConfig is null)
            throw new ConfigurationNullException(nameof(rsbCacheConfig));

        RsbConfiguration.Cache = rsbCacheConfig;

        return hostBuilder;
    }

    public static IHostBuilder UseDataStorageSagaPersistence<TSettings>(
        this IHostBuilder hostBuilder)
        where TSettings : class, IConfigureDataStorageSagaPersistence, new()
    {
        hostBuilder.ConfigureServices((hostBuilderContext, _) =>
        {
            var settings = ConfigProvider.LoadSettings<TSettings>(hostBuilderContext.Configuration);

            RsbConfiguration.DataStorageSagaPersistence = new DataStorageSagaPersistenceConfig
            {
                DataStorageConnectionString = settings.DataStorageConnectionString,
                DataStorageContainer = settings.DataStorageContainer
            };
        });

        return UseDataStorageSagaPersistence(hostBuilder);
    }

    public static IHostBuilder UseDataStorageSagaPersistence(
        this IHostBuilder hostBuilder, DataStorageSagaPersistenceConfig config)
    {
        if (config is null)
            throw new ConfigurationNullException(nameof(config));

        RsbConfiguration.DataStorageSagaPersistence = config;

        return UseDataStorageSagaPersistence(hostBuilder);
    }

    private static IHostBuilder UseDataStorageSagaPersistence(
        IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices((_, services) =>
        {
            services
                .AddScoped<IAzureDataStorageService>(
                    x =>
                        new AzureDataStorageService(RsbConfiguration
                            .DataStorageSagaPersistence?
                            .DataStorageConnectionString!))
                .AddScoped<ISagaPersistenceService,
                    SagaDataStoragePersistenceService>();
        });
    }

    public static IHostBuilder UseSqlServerSagaPersistence<TSettings>(
        this IHostBuilder hostBuilder)
        where TSettings : class, IConfigureSqlServerSagaPersistence, new()
    {
        hostBuilder.ConfigureServices((hostBuilderContext, _) =>
        {
            var settings = ConfigProvider.LoadSettings<TSettings>(hostBuilderContext.Configuration);

            RsbConfiguration.SqlServerSagaPersistence = new SqlServerSagaPersistenceConfig
            {
                ConnectionString = settings.ConnectionString
            };
        });

        return UseSqlServerSagaPersistence(hostBuilder);
    }

    public static IHostBuilder UseSqlServerSagaPersistence(
        this IHostBuilder hostBuilder, SqlServerSagaPersistenceConfig config)
    {
        if (config is null)
            throw new ConfigurationNullException(nameof(config));

        RsbConfiguration.SqlServerSagaPersistence = config;

        return UseSqlServerSagaPersistence(hostBuilder);
    }

    private static IHostBuilder UseSqlServerSagaPersistence(
        IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices((_, services) =>
        {
            services
                .AddScoped<ISqlServerService, SqlServerService>()
                .AddScoped<ISagaPersistenceService, SagaSqlServerPersistenceService>();
        });
    }
}