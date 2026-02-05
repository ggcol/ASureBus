using ASureBus.Services.ServiceBus;

namespace ASureBus.Core.TypesHandling;

internal static class QueueName
{
    internal static string Resolve(Type type)
    {
        var isGeneric = type.IsGenericType;

        return isGeneric
            ? type.Name.Split('`')[0] + "_" + string.Join('_', type.GenericTypeArguments.Select(x => x.Name))
            : type.Name;
    }
    
    internal static string ForConsumerScopedTopicSubscription(TopicConfiguration config)
    {
        return string.Join('-', config.Name, config.SubscriptionName);
    }
}