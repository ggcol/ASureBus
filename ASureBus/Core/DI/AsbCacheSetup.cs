using ASureBus.Abstractions.Configurations;
using ASureBus.ConfigurationObjects.Config;
using ASureBus.ConfigurationObjects.Exceptions;
using ASureBus.Utils;
using Microsoft.Extensions.Hosting;

namespace ASureBus.Core.DI;

public static class AsbCacheSetup
{
    public static IHostBuilder ConfigureAsbCache<TSettings>(
        this IHostBuilder hostBuilder)
        where TSettings : class, IConfigureAsbCache, new()
    {
        return hostBuilder.ConfigureServices((hostBuilderContext, _) =>
        {
            var settings = ConfigProvider.LoadSettings<TSettings>(hostBuilderContext.Configuration);

            AsbConfiguration.Cache = new AsbCacheConfig
            {
                Expiration = settings.Expiration,
                TopicConfigPrefix = settings.TopicConfigPrefix,
                ServiceBusSenderCachePrefix = settings.ServiceBusSenderCachePrefix
            };
        });
    }

    public static IHostBuilder ConfigureAsbCache(this IHostBuilder hostBuilder,
        AsbCacheConfig asbCacheConfig)
    {
        if (asbCacheConfig is null)
            throw new ConfigurationNullException(nameof(asbCacheConfig));

        AsbConfiguration.Cache = asbCacheConfig;

        return hostBuilder;
    }
    
    public static IHostBuilder ConfigureAsbCache(
        this IHostBuilder hostBuilder, Action<AsbCacheConfig> options)
    {
        var opt = new AsbCacheConfig();
        options(opt);
        
        AsbConfiguration.Cache = new AsbCacheConfig
        {
            Expiration = opt.Expiration,
            TopicConfigPrefix = opt.TopicConfigPrefix,
            ServiceBusSenderCachePrefix = opt.ServiceBusSenderCachePrefix
        };

        return hostBuilder;
    }
}