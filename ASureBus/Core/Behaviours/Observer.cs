using System.Collections.Concurrent;
using ASureBus.Abstractions.Behaviours;

namespace ASureBus.Core.Behaviours;

internal abstract class Observer<T> where T : ObservableExpirable
{
    private readonly ConcurrentDictionary<object, T> _shelf = new();

    protected void Observe(object key, T observable, EventHandler? onExpired)
    {
        observable.Expired += onExpired;
        observable.Expired += (_, _) =>
        {
            _shelf.TryRemove(key, out _);
        };
        
        _shelf[key] = observable;
    }
}