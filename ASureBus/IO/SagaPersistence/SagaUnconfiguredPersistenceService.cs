using ASureBus.Core.TypesHandling.Entities;
using ASureBus.IO.SagaPersistence.Exceptions;

namespace ASureBus.IO.SagaPersistence;

internal sealed class SagaUnconfiguredPersistenceService : ISagaPersistenceService
{
    public Task<object?> Get(SagaType sagaType, Guid correlationId, CancellationToken cancellationToken = default)
    {
        throw new UnconfiguredSagaPersistenceException();
    }

    public Task Save<TItem>(TItem item, SagaType sagaType, Guid correlationId, CancellationToken cancellationToken = default)
    {
        throw new UnconfiguredSagaPersistenceException();
    }

    public Task Delete(SagaType sagaType, Guid correlationId, CancellationToken cancellationToken = default)
    {
        throw new UnconfiguredSagaPersistenceException();
    }
}