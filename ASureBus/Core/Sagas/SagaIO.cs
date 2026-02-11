using ASureBus.Core.TypesHandling.Entities;
using ASureBus.IO.SagaPersistence;

namespace ASureBus.Core.Sagas;

// ReSharper disable once InconsistentNaming
internal class SagaIO(ISagaPersistenceService persistenceService) : ISagaIO
{
    private readonly bool _isInUse = AsbConfiguration.OffloadSagas;

    public async Task<object?> Load(Guid correlationId, SagaType sagaType,
        CancellationToken cancellationToken = default)
    {
        if (!_isInUse) return null;

        return await persistenceService!.Get(sagaType, correlationId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Unload(object? implSaga, Guid correlationId,
        SagaType sagaType, CancellationToken cancellationToken = default)
    {
        if (!_isInUse) return;

        await persistenceService!.Save(implSaga, sagaType, correlationId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Delete(Guid correlationId, SagaType sagaType,
        CancellationToken cancellationToken = default)
    {
        if (!_isInUse) return;

        await persistenceService!.Delete(sagaType, correlationId, cancellationToken)
            .ConfigureAwait(false);
    }
}