using ASureBus.Abstractions.Configurations;
using ASureBus.ConfigurationObjects.Config;
using ASureBus.ConfigurationObjects.Exceptions;
using ASureBus.ConfigurationObjects.Options;
using ASureBus.IO.FileSystem;
using ASureBus.IO.SagaPersistence;
using ASureBus.IO.SqlServer;
using ASureBus.IO.SqlServer.DbConnection;
using ASureBus.IO.StorageAccount;
using ASureBus.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ASureBus.Core.DI;

public static class SagaPersistenceSetup
{
    public static IHostBuilder UseDataStorageSagaPersistence<TSettings>(this IHostBuilder hostBuilder)
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

        return ConfigureDataStoragePersistenceDependencies(hostBuilder);
    }

    public static IHostBuilder UseDataStorageSagaPersistence(this IHostBuilder hostBuilder, 
        DataStorageSagaPersistenceConfig config)
    {
        if (config is null)
            throw new ConfigurationNullException(nameof(config));

        AsbConfiguration.DataStorageSagaPersistence = config;

        return ConfigureDataStoragePersistenceDependencies(hostBuilder);
    }

    public static IHostBuilder UseDataStorageSagaPersistence(this IHostBuilder hostBuilder, 
        Action<DataStorageSagaPersistenceOptions> options)
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

        return ConfigureDataStoragePersistenceDependencies(hostBuilder);
    }

    private static IHostBuilder ConfigureDataStoragePersistenceDependencies(IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices((_, services) =>
        {
            RemovePersistencePlaceholderService(services);
            
            services
                .AddScoped<IAzureDataStorageService>(
                    _ => new AzureDataStorageService(AsbConfiguration.DataStorageSagaPersistence?.ConnectionString!))
                .AddScoped<ISagaPersistenceService, SagaDataStoragePersistenceService>();
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

        return ConfigureSqlServerPersistenceDependencies(hostBuilder);
    }

    public static IHostBuilder UseSqlServerSagaPersistence(
        this IHostBuilder hostBuilder, SqlServerSagaPersistenceConfig config)
    {
        if (config is null)
            throw new ConfigurationNullException(nameof(config));

        AsbConfiguration.SqlServerSagaPersistence = config;

        return ConfigureSqlServerPersistenceDependencies(hostBuilder);
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

        return ConfigureSqlServerPersistenceDependencies(hostBuilder);
    }

    private static IHostBuilder ConfigureSqlServerPersistenceDependencies(
        IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices((_, services) =>
        {
            RemovePersistencePlaceholderService(services);
            
            services
                .AddSingleton<IDbConnectionFactory, SqlServerConnectionFactory>()
                .AddScoped<ISqlServerService, SqlServerService>()
                .AddScoped<ISagaPersistenceService, SagaSqlServerPersistenceService>();
        });
    }

    public static IHostBuilder UseFileSystemSagaPersistence<TSettings>(this IHostBuilder hostBuilder)
        where TSettings : class, IConfigureFileSystemSagaPersistence, new()
    {
        hostBuilder.ConfigureServices((hostBuilderContext, services) =>
        {
            var settings = ConfigProvider.LoadSettings<TSettings>(hostBuilderContext.Configuration);

            AsbConfiguration.FileSystemSagaPersistence = new FileSystemSagaPersistenceConfig()
            {
                RootDirectoryPath = settings.RootDirectoryPath
            };
        });

        return ConfigureFileSystemPersistenceDependencies(hostBuilder);
    }

    public static IHostBuilder UseFileSystemSagaPersistence(this IHostBuilder hostBuilder,
        FileSystemSagaPersistenceConfig config)
    {
        if (config is null) throw new ConfigurationNullException(nameof(config));

        AsbConfiguration.FileSystemSagaPersistence = config;

        return ConfigureFileSystemPersistenceDependencies(hostBuilder);
    }

    public static IHostBuilder UseFileSystemSagaPersistence(this IHostBuilder hostBuilder,
        Action<FileSystemSagaPersistenceOptions> options)
    {
        var opt = new FileSystemSagaPersistenceOptions();
        options.Invoke(opt);

        if (string.IsNullOrWhiteSpace(opt.RootDirectoryPath))
            throw new ConfigurationNullException(nameof(FileSystemSagaPersistenceOptions.RootDirectoryPath));

        AsbConfiguration.FileSystemSagaPersistence = new FileSystemSagaPersistenceConfig
        {
            RootDirectoryPath = opt.RootDirectoryPath
        };

        return ConfigureFileSystemPersistenceDependencies(hostBuilder);
    }

    public static IHostBuilder UseFileSystemSagaPersistence(this IHostBuilder hostBuilder)
    {
        AsbConfiguration.FileSystemSagaPersistence = new FileSystemSagaPersistenceConfig();
        return ConfigureFileSystemPersistenceDependencies(hostBuilder);
    }

    private static IHostBuilder ConfigureFileSystemPersistenceDependencies(IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices(services =>
        {
            RemovePersistencePlaceholderService(services);

            services.AddSingleton<IFileSystemService, FileSystemService>();
            services.AddSingleton<ISagaPersistenceService, SagaFileSystemPersistenceService>();
        });
    }

    private static void RemovePersistencePlaceholderService(IServiceCollection services)
    {
        if (services.Count <= 0) return;
        
        var service = services.SingleOrDefault(x => x.ServiceType == typeof(ISagaPersistenceService));
        if (service is not null)
        {
            services.Remove(service);
        }
    }
}