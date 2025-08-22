using ASureBus.Abstractions.Configurations;

namespace ASureBus.ConfigurationObjects.Config;

public sealed class AsbCacheConfig : IConfigureAsbCache
{
    private TimeSpan? _expiration;
    private string? _topicConfigPrefix;
    private string? _serviceBusSenderCachePrefix;

    public TimeSpan? Expiration
    {
        get => _expiration ?? Defaults.Cache.EXPIRATION;
        set => _expiration = value;
    }

    public string? TopicConfigPrefix
    {
        get => _topicConfigPrefix ?? Defaults.Cache.TOPIC_CONFIG_PREFIX;
        set => _topicConfigPrefix = value;
    }

    public string? ServiceBusSenderCachePrefix
    {
        get => _serviceBusSenderCachePrefix ?? Defaults.Cache.SERVICE_BUS_SENDER_PREFIX;
        set => _serviceBusSenderCachePrefix = value;
    }
}
