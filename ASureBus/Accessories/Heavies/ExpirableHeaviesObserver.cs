using ASureBus.Abstractions;
using ASureBus.Accessories.Heavies.Entities;
using ASureBus.Core.Behaviours;

namespace ASureBus.Accessories.Heavies;

internal sealed class ExpirableHeaviesObserver : Observer<Heavy>, IExpirableHeaviesObserver
{
    public void DeleteOnExpiration(Heavy heavy, Guid messageId, CancellationToken cancellationToken = default)
    {
        Observe(heavy.Ref, heavy, async void (_, _) =>
        {
            await HeavyIo.Delete(messageId, heavy.Ref, cancellationToken).ConfigureAwait(false);
        });
    }
}