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

using ASureBus.ConfigurationObjects;
using ASureBus.ConfigurationObjects.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Playground.Samples._06_SagaPersistence;
using Playground.Settings;
using ASureBus.Core.DI;
using Playground;
using Playground.Samples._01_OneCommand;
using Playground.Samples._02_OneEvent;
using Playground.Samples._03_TwoMessagesSameHandlerClass;
using Playground.Samples._04_ASaga;
using Playground.Samples._05_Heavy;
using Playground.Samples._07_DelayedAndScheduled;
using Playground.Samples._08_ABrokenSaga;
using Playground.Samples._09_LongerSaga;
using Playground.Samples._10_SagaWithTimeout;
using Playground.Samples._11_SagaTimeoutTriggeredAfterCompleting;

await Host
    .CreateDefaultBuilder()
    
    
    /*
     * ========================================
     * MINIMAL SETUP
     * ========================================
     */

    // Configure the application to use Azure Service Bus with the specified settings
    // .UseAsb<WholeServiceBusSettings>()
    // .UseAsb<PartialServiceBusSettings>()
    
    // Configure the application to use Azure Service Bus with a custom configuration
    // .UseAsb(new ServiceBusConfig
    // {
    //     ConnectionString = "connection-string",
    //     // All the following are optional, they are initialized as default if not mentioned
    //     TransportType = "", // Default is "AmqpWebSocket"
    //     MaxRetries = 0, // Default is 3
    //     DelayInSeconds = 0, // Default is 0.8
    //     MaxDelayInSeconds = 0, // Default is 60
    //     TryTimeoutInSeconds = 0, // Default is 300
    //     ServiceBusRetryMode = "", // Default is "Fixed"
    // })
    
    // Configure the application to use Azure Service Bus using delegate
    .UseAsb((opt) =>
    {
        opt.ConnectionString = "connection-string";
        // All the following are optional, they are initialized as default if not mentioned
        // opt.TransportType = ""; // Default is "AmqpWebSocket"
        // opt.MaxRetries = 0; // Default is 3
        // opt.DelayInSeconds = 0; // Default is 0.8
        // opt.MaxDelayInSeconds = 0; // Default is 60
        // opt.TryTimeoutInSeconds = 0; // Default is 300
        // opt.ServiceBusRetryMode = ""; // Default is "Fixed"
    })
    
    
    
    /*
     * ========================================
     * ASB single behavior setup
     * ========================================
     */
    
    // MAX CONCURRENT CALLS
    
    // .ConfigureMaxConcurrentCalls(opt =>
    // {
    //     opt.MaxConcurrentCalls = 100; // Default is 20
    // })
    
    
    // MESSAGE LOCK HANDLING
    
    // .ConfigureMessageLockHandling(opt =>
    // {
    //     opt.EnableMessageLockAutoRenewal = true; // Default is false
    //     opt.MessageLockRenewalPreemptiveThresholdInSeconds = 30; // Default is 10
    //     opt.MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(20); // Default is 5 minutes
    // })
    
    
    // SERVICE BUS CLIENT OPTIONS
    
    // .ConfigureServiceBusClientOptions(opt =>
    // {
    //     //se ms-doc for ServiceBusClientOptions
    // })
    
    
    
    /*
     * ========================================
     * HEAVY PROPS SETUP
     * ========================================
     */

    // Configure the application to use heavy properties with the specified settings
    // .UseHeavyProps<HeavySettings>()

    // Configure the application to use heavy properties with a custom configuration
    // .UseHeavyProps(new HeavyPropertiesConfig()
    // {
    //     ConnectionString = "",
    //     Container = ""
    // })
    
    // Configure the application to use heavy properties using delegate
    // .UseHeavyProps((opt) =>
    // {
    //     opt.ConnectionString = "";
    //     opt.Container = "";
    // })
    
    
    /*
     * ========================================
     * SAGA PERSISTENCE SETUP
     * ========================================
     */

    // Configure the application to use data storage for saga persistence with the specified settings
    // .UseDataStorageSagaPersistence<DataStorageSagaPersistenceSettings>()
    
    // Configure the application to use data storage for saga persistence with a custom configuration
    // .UseDataStorageSagaPersistence(new DataStorageSagaPersistenceConfig()
    // {
    //     ConnectionString = "connection-string",
    //     Container = "container-name"
    // })
    
    // Configure the application to use data storage for saga persistence using delegate
    // .UseDataStorageSagaPersistence((opt) =>
    // {
    //     opt.ConnectionString = "connection-string";
    //     opt.Container = "container-name";
    // })

    // Configure the application to use SQL Server for saga persistence with the specified settings
    // .UseSqlServerSagaPersistence<SqlServerSagaPersistenceSettings>()
    
    // Configure the application to use SQL Server for saga persistence with a custom configuration
    // .UseSqlServerSagaPersistence(new SqlServerSagaPersistenceConfig()
    // {
    //     ConnectionString = "connection-string",
    //     Schema = "schema-name"
    // })
    
    // Configure the application to use SQL Server for saga persistence using delegate
    // .UseSqlServerSagaPersistence((opt) =>
    // {
    //     opt.ConnectionString = "connection-string";
    //     opt.Schema = "schema-name"; // Optional, defaults to "sagas"
    // })
    
    
    
    /*
     * ========================================
     * CONFIGURE APPLICATION SERVICES
     * ========================================
     */

    // Configure the application's services
    .ConfigureServices(
        (_, services) =>
        {
            services.AddHostedService<OneCommandInitJob>();
            // services.AddHostedService<OneEventInitJob>();
            // services.AddHostedService<TwoMessagesSameHandlerClassInitJob>();
            // services.AddHostedService<ASagaInitJob>();
            // services.AddHostedService<AGenericJob>();
            // services.AddHostedService<HeavyInitJob>();
            // services.AddHostedService<APersistedSagaInitJob>();
            // services.AddHostedService<DelayedAndScheduledInitJob>();
            // services.AddHostedService<ABrokenSagaInitJob>();
            // services.AddHostedService<LongerSagaInitJob>();
            // services.AddHostedService<SagaWithTimeoutInitJob>();
            // services.AddHostedService<SagaTimeoutTriggeredAfterCompletingInitJob>();
            // services.AddHostedService<GenericTypeMessagesInitJob>();
            
            services.AddLogging();
        })
    
    
    
    /*
     * ========================================
     * CACHE SETUP
     * ========================================
     */

    // Configure the application to use Azure Service Bus cache with the specified settings
    // .ConfigureAsbCache<WholeCacheSettings>()
    // .ConfigureAsbCache<PartialCacheSettings>()

    // Configure the application to use Azure Service Bus cache with a custom configuration
    // .ConfigureAsbCache(new AsbCacheConfig()
    // {
    //     // All these 3 are optional, they are initialized as default if not mentioned
    //     Expiration = TimeSpan.FromHours(2), // default is 5 minutes
    //     TopicConfigPrefix = "", // default is "topicConfig"
    //     ServiceBusSenderCachePrefix = "" // default is "sender"
    // })
    
    // Configure the application to use Azure Service Bus cache using delegate
    // .ConfigureAsbCache((opt) =>
    // {
    //     // All these 3 are optional, they are initialized as default if not mentioned
    //     opt.Expiration = TimeSpan.FromHours(2); // default is 5 minutes
    //     opt.TopicConfigPrefix = ""; // default is "topicConfig"
    //     opt.ServiceBusSenderCachePrefix = ""; // default is "sender"
    // })
    
    
    
    /*
     * ========================================
     * RUN THE APPLICATION
     * ========================================
     */
    
    .RunConsoleAsync();