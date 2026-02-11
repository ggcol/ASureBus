using ASureBus.Core.Sagas;
using ASureBus.Core.TypesHandling.Entities;
using ASureBus.IO.StorageAccount;

namespace ASureBus.IO.SagaPersistence;

internal sealed class SagaDataStoragePersistenceService(
    IAzureDataStorageService storage,
    IServiceProvider services)
    : ISagaPersistenceService
{
    public async Task<object?> Get(SagaType sagaType, Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        return await storage.Get(
                AsbConfiguration.DataStorageSagaPersistence?.Container!,
                GetName(sagaType, correlationId),
                sagaType.Type,
                new SagaConverter(sagaType.Type, sagaType.SagaDataType, services),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Save<TItem>(TItem item, SagaType sagaType,
        Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        await storage.Save(
                item,
                AsbConfiguration.DataStorageSagaPersistence?.Container!,
                GetName(sagaType, correlationId),
                true,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Delete(SagaType sagaType, Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        await storage.Delete(
                AsbConfiguration.DataStorageSagaPersistence?.Container!,
                GetName(sagaType, correlationId),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static string GetName(SagaType sagaType, Guid correlationId)
    {
        return $"{sagaType.Type.Name}/{correlationId}";
    }
}