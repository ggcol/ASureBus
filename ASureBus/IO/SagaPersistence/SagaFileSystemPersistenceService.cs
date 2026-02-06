using ASureBus.Core.Sagas;
using ASureBus.Core.TypesHandling.Entities;
using ASureBus.IO.FileSystem;

namespace ASureBus.IO.SagaPersistence;

internal sealed class SagaFileSystemPersistenceService(IFileSystemService storage, IServiceProvider services)
    : ISagaPersistenceService
{
    public async Task<object?> Get(SagaType sagaType, Guid correlationId, CancellationToken cancellationToken = default)
    {
        return await storage
            .Get(sagaType.Type.Name,
                correlationId.ToString(),
                sagaType.Type,
                new SagaConverter(sagaType.Type, sagaType.SagaDataType, services),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Save<TItem>(TItem item, SagaType sagaType, Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        await storage.Save(item, sagaType.Type.Name, correlationId.ToString(), cancellationToken).ConfigureAwait(false);
    }

    public Task Delete(SagaType sagaType, Guid correlationId, CancellationToken cancellationToken = default)
    {
        storage.Delete(sagaType.Type.Name, correlationId.ToString());
        return Task.CompletedTask;
    }
}