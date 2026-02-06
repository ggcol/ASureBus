using ASureBus.Abstractions.Configurations;
using ASureBus.ConfigurationObjects.Config;
using ASureBus.ConfigurationObjects.Exceptions;
using ASureBus.ConfigurationObjects.Options;
using ASureBus.IO.SagaPersistence;
using ASureBus.IO.SqlServer;
using ASureBus.IO.StorageAccount;
using ASureBus.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ASureBus.Core.DI;

public static class SagaPersistenceSetup
{
    public static IHostBuilder UseDataStorageSagaPersistence<TSettings>(
        this IHostBuilder hostBuilder)
        where TSettings : class, IConfigureDataStorageSagaPersistence, new()
    {
        hostBuilder.ConfigureServices((hostBuilderContext, _) =>
        {
            var settings = ConfigProvider.LoadSettings<TSettings>(hostBuilderContext.Configuration);

            AsbConfiguration.DataStorageSagaPersistence = new DataStorageSagaPersistenceConfig
            {
                ConnectionString = settings.ConnectionString,
                Container = settings.Container
            };
        });

        return UseDataStorageSagaPersistence(hostBuilder);
    }

    public static IHostBuilder UseDataStorageSagaPersistence(
        this IHostBuilder hostBuilder, DataStorageSagaPersistenceConfig config)
    {
        if (config is null)
            throw new ConfigurationNullException(nameof(config));

        AsbConfiguration.DataStorageSagaPersistence = config;

        return UseDataStorageSagaPersistence(hostBuilder);
    }
    
    public static IHostBuilder UseDataStorageSagaPersistence(
        this IHostBuilder hostBuilder, Action<DataStorageSagaPersistenceOptions> options)
    {
        var opt = new DataStorageSagaPersistenceOptions();
        options.Invoke(opt);

        if (string.IsNullOrWhiteSpace(opt.ConnectionString))
            throw new ConfigurationNullException(nameof(DataStorageSagaPersistenceOptions.ConnectionString));

        if (string.IsNullOrWhiteSpace(opt.Container))
            throw new ConfigurationNullException(nameof(DataStorageSagaPersistenceOptions.Container));

        AsbConfiguration.DataStorageSagaPersistence = new DataStorageSagaPersistenceConfig
        {
            ConnectionString = opt.ConnectionString,
            Container = opt.Container
        };

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
                        new AzureDataStorageService(AsbConfiguration
                            .DataStorageSagaPersistence?
                            .ConnectionString!))
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

            AsbConfiguration.SqlServerSagaPersistence = new SqlServerSagaPersistenceConfig
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

        AsbConfiguration.SqlServerSagaPersistence = config;

        return UseSqlServerSagaPersistence(hostBuilder);
    }
    
    public static IHostBuilder UseSqlServerSagaPersistence(
        this IHostBuilder hostBuilder, Action<SqlServerSagaPersistenceOptions> options)
    {
        var opt = new SqlServerSagaPersistenceOptions();
        options(opt);

        if (string.IsNullOrWhiteSpace(opt.ConnectionString))
            throw new ConfigurationNullException(nameof(SqlServerSagaPersistenceOptions.ConnectionString));

        AsbConfiguration.SqlServerSagaPersistence = new SqlServerSagaPersistenceConfig
        {
            ConnectionString = opt.ConnectionString,
            Schema = opt.Schema
        };

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