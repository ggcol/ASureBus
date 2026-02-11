using ASureBus.Abstractions;
using ASureBus.Accessories.Heavies.Entities;

namespace ASureBus.IO.Heavies;

internal interface IHeavyIO
{
    internal bool IsHeavyConfigured { get; }
    
    internal Task<IReadOnlyList<HeavyReference>> Unload<TMessage>(TMessage message, Guid messageId, 
        CancellationToken cancellationToken = default)
        where TMessage : IAmAMessage;

    internal Task Load(object message, IReadOnlyList<HeavyReference> heavies, Guid messageId, 
        CancellationToken cancellationToken = default);

    internal Task Delete(Guid messageId, Guid heavyReference, CancellationToken cancellationToken = default);
}