using System.Collections.Concurrent;
using ASureBus.Core.Caching.Entities;

namespace ASureBus.Core.Caching;

internal sealed class AsbCache : IAsbCache
{
    private readonly ConcurrentDictionary<object, ObservableCacheItem?> _shelf = new();

    public T? Set<T>(object key, T? obj, TimeSpan? expiresAfter = null)
    {
        var obs = new ObservableCacheItem(key, obj, expiresAfter);

        if (obs.HasExpiration)
        {
            obs.Expired += (_, _) => { _shelf.TryRemove(obs.Key, out _); };
        }

        _shelf[key] = obs;
        return obj;
    }

    public object? Set(object key, object? obj, TimeSpan? expiresAfter = null)
    {
        return Set<object>(key, obj, expiresAfter);
    }

    public T? Upsert<T>(object key, T? obj, TimeSpan? expiresAfter = null)
    {
        _shelf.TryRemove(key, out _);
        return Set(key, obj, expiresAfter);
    }

    public object? Upsert(object key, object? obj, TimeSpan? expiresAfter = null)
    {
        return Upsert<object>(key, obj, expiresAfter);
    }

    public bool TryGetValue<T>(object key, out T? retrieved)
    {
        var exists = _shelf.TryGetValue(key, out var obs);

        if (exists)
        {
            retrieved = (T?)obs?.CachedValue;
            return true;
        }

        retrieved = default;
        return false;
    }

    public bool TryGetValue(object key, out object? retrieved)
    {
        return TryGetValue<object>(key, out retrieved);
    }

    public void Remove(object key)
    {
        _shelf.TryRemove(key, out _);
    }
}