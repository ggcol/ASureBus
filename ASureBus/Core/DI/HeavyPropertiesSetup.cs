﻿using ASureBus.Abstractions.Configurations;
using ASureBus.Accessories.Heavy;
using ASureBus.ConfigurationObjects;
using ASureBus.ConfigurationObjects.Exceptions;
using ASureBus.Services.StorageAccount;
using ASureBus.Utils;
using Microsoft.Extensions.Hosting;

namespace ASureBus.Core.DI;

public static class HeavyPropertiesSetup
{
    public static IHostBuilder UseHeavyProps<TSettings>(
        this IHostBuilder hostBuilder)
        where TSettings : class, IConfigureHeavyProperties, new()
    {
        hostBuilder.ConfigureServices((hostBuilderContext, _) =>
        {
            var settings = ConfigProvider.LoadSettings<TSettings>(hostBuilderContext.Configuration);

            AsbConfiguration.HeavyProps = new HeavyPropertiesConfig
            {
                ConnectionString = settings.ConnectionString,
                Container = settings.Container
            };
            ConfigureStorage();
        });


        return hostBuilder;
    }

    public static IHostBuilder UseHeavyProps(
        this IHostBuilder hostBuilder, HeavyPropertiesConfig? heavyPropsConfig)
    {
        if (heavyPropsConfig is null)
            throw new ConfigurationNullException(nameof(HeavyPropertiesConfig));

        AsbConfiguration.HeavyProps = heavyPropsConfig;

        ConfigureStorage();

        return hostBuilder;
    }

    private static void ConfigureStorage()
    {
        HeavyIo.ConfigureStorage(
            new AzureDataStorageService(AsbConfiguration.HeavyProps!.ConnectionString));
    }
}