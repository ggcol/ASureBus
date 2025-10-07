using ASureBus.ConfigurationObjects;
using ASureBus.ConfigurationObjects.Config;
using ASureBus.ConfigurationObjects.Options;

namespace ASureBus.Core;

internal static class AsbConfiguration
{
    //basic configuration
    public static InternalServiceBusConfig ServiceBus { get; set; } = null!;
    public static AsbCacheConfig Cache { get; set; } = new();
    public static MessageLockRenewalOptions MessageLockOptions { get; set; } = new();
    //heavy props configuration
    internal static bool UseHeavyProperties => HeavyProps is not null;
    public static HeavyPropertiesConfig? HeavyProps { get; set; }

    //saga offloading configuration
    internal static bool OffloadSagas => UseDataStorageSagaPersistence || UseSqlServerSagaPersistence;
    internal static bool UseDataStorageSagaPersistence => DataStorageSagaPersistence is not null;
    internal static bool UseSqlServerSagaPersistence => SqlServerSagaPersistence is not null;
    public static DataStorageSagaPersistenceConfig? DataStorageSagaPersistence { get; set; }
    public static SqlServerSagaPersistenceConfig? SqlServerSagaPersistence { get; set; }

    //message lock renewal configuration
    public static bool EnableMessageLockAutoRenewal
        => MessageLockOptions.EnableMessageLockAutoRenewal.HasValue
           && MessageLockOptions.EnableMessageLockAutoRenewal.Value;
    
    // message concurrency configuration
    public static int MaxConcurrentCalls { get; set; } = Defaults.ServiceBus.MAX_CONCURRENT_CALLS;
}