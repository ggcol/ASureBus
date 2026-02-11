using ASureBus.Abstractions;
using ASureBus.IO.Heavies;

namespace ASureBus.Accessories.Heavies;

internal interface IExpirableHeaviesObserver
{
    internal void DeleteOnExpiration(Heavy heavy, Guid messageId, IHeavyIO heavyIO, 
        CancellationToken cancellationToken = default);
}