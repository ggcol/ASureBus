using ASureBus.Core.Entities;

namespace ASureBus.Core.Caching.Entities;

internal sealed class ObservableCacheItem(
    object key,
    object? cachedValue,
    TimeSpan? expiresAfter = null)
    : ObservableExpirable(expiresAfter)
{
    internal object Key { get; } = key;
    internal object? CachedValue { get; } = cachedValue;
}