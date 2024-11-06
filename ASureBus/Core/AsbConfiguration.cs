﻿// ReSharper disable InconsistentNaming

using ASureBus.Abstractions.Configurations.ConfigObjects;

namespace ASureBus.Core;

internal static class AsbConfiguration
{
    //basic configuration
    public static ServiceBusConfig ServiceBus { get; set; } = new();
    public static AsbCacheConfig Cache { get; set; } = new();

    //heavy props configuration
    internal static bool UseHeavyProperties => HeavyProps is not null;
    public static HeavyPropertiesConfig? HeavyProps { get; set; }

    //saga offloading configuration
    internal static bool OffloadSagas =>
        UseDataStorageSagaPersistence || UseSqlServerSagaPersistence;
    internal static bool UseDataStorageSagaPersistence => DataStorageSagaPersistence is not null;
    internal static bool UseSqlServerSagaPersistence => SqlServerSagaPersistence is not null;
    public static DataStorageSagaPersistenceConfig? DataStorageSagaPersistence { get; set; }
    public static SqlServerSagaPersistenceConfig? SqlServerSagaPersistence { get; set; }
}