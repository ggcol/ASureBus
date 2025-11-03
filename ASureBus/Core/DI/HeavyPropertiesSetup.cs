using ASureBus.Abstractions.Configurations;
using ASureBus.Accessories.Heavies;
using ASureBus.ConfigurationObjects.Config;
using ASureBus.ConfigurationObjects.Exceptions;
using ASureBus.ConfigurationObjects.Options;
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

    public static IHostBuilder UseHeavyProps(
        this IHostBuilder hostBuilder, Action<HeavyPropertiesOptions> options)
    {
        var opt = new HeavyPropertiesOptions();
        options(opt);

        if (string.IsNullOrWhiteSpace(opt.ConnectionString))
            throw new ConfigurationNullException(nameof(HeavyPropertiesOptions.ConnectionString));
        
        if (string.IsNullOrWhiteSpace(opt.Container))
            throw new ConfigurationNullException(nameof(HeavyPropertiesOptions.Container));
        
        AsbConfiguration.HeavyProps = new HeavyPropertiesConfig
        {
            ConnectionString = opt.ConnectionString,
            Container = opt.Container
        };
        
        ConfigureStorage();
        
        return hostBuilder;
    }

    private static void ConfigureStorage()
    {
        HeavyIo.ConfigureStorage(
            new AzureDataStorageService(AsbConfiguration.HeavyProps!.ConnectionString));
    }
}