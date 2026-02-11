using ASureBus.Abstractions;
using ASureBus.Accessories.Heavies.Entities;
using ASureBus.IO.Heavies.Exceptions;

namespace ASureBus.IO.Heavies;

internal sealed class UnconfiguredHeavyIO : IHeavyIO
{
    public bool IsHeavyConfigured => false;

    public Task<IReadOnlyList<HeavyReference>> Unload<TMessage>(TMessage message, Guid messageId,
        CancellationToken cancellationToken = default)
        where TMessage : IAmAMessage
    {
        throw new UnconfiguredHeavyIOException();
    }

    public Task Load(object message, IReadOnlyList<HeavyReference> heavies, Guid messageId,
        CancellationToken cancellationToken = default)
    {
        throw new UnconfiguredHeavyIOException();
    }

    public Task Delete(Guid messageId, Guid heavyReference, CancellationToken cancellationToken = default)
    {
        throw new UnconfiguredHeavyIOException();
    }
}