using ASureBus.Abstractions;

namespace ASureBus.Accessories.Heavies;

internal interface IExpirableHeaviesObserver
{
    internal void DeleteOnExpiration(Heavy heavy, Guid messageId, CancellationToken cancellationToken = default);
}