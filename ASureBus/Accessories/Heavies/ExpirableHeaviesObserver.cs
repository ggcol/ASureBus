using ASureBus.Abstractions;
using ASureBus.Core.Behaviours;
using ASureBus.IO.Heavies;

namespace ASureBus.Accessories.Heavies;

internal sealed class ExpirableHeaviesObserver : Observer<Heavy>, IExpirableHeaviesObserver
{
    public void DeleteOnExpiration(Heavy heavy, Guid messageId, IHeavyIO heavyIO, 
        CancellationToken cancellationToken = default)
    {
        Observe(heavy.Ref, heavy, async void (_, _) =>
        {
            await heavyIO.Delete(messageId, heavy.Ref, cancellationToken).ConfigureAwait(false);
        });
    }
}