﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Playground.Samples._05_Heavy;
using Playground.Samples._06_SagaPersistence;
using Playground.Settings;
using Asb.Core;

await Host
    .CreateDefaultBuilder()
    .UseRsb<ServiceBusSettings>()
    // .UseRsb(new ServiceBusConfig()
    // {
    //     ServiceBusConnectionString = "",
    //     ClientOptions = new()
    //     {
    //         TransportType = ServiceBusTransportType.AmqpWebSockets
    //     }
    // })
    // .UseHeavyProps<HeavySettings>()
    // .UseHeavyProps(new HeavyPropertiesConfig()
    // {
    //     DataStorageConnectionString = "",
    //     DataStorageContainer = ""
    // })
    .UseDataStorageSagaPersistence<DataStorageSagaPersistenceSettings>()
    // .UseSqlServerSagaPersistence<SqlServerSagaPersistenceSettings>()
    .ConfigureServices(
        (_, services) =>
        {
            // services.AddHostedService<OneCommandInitJob>();
            // services.AddHostedService<OneEventInitJob>();
            // services.AddHostedService<TwoMessagesSameHandlerClassInitJob>();
            // services.AddHostedService<ASagaInitJob>();
            // services.AddHostedService<AGenericJob>();
            // services.AddHostedService<HeavyInitJob>();
            services.AddHostedService<APersistedSagaInitJob>();
            services.AddLogging();
        })
    // .ConfigureRsbCache<WholeCacheSettings>()
    // .ConfigureRsbCache<PartialCacheSettings>()
    // .ConfigureRsbCache(new RsbCacheConfig()
    // {
    //     //all these 3 are optional, they are init as default if not mentioned
    //     Expiration = TimeSpan.FromHours(2),
    //     TopicConfigPrefix = "",
    //     ServiceBusSenderCachePrefix = ""
    // })
    .RunConsoleAsync();