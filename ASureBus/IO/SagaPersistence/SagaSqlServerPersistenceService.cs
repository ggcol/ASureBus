using ASureBus.Core.Sagas;
using ASureBus.Core.TypesHandling.Entities;
using ASureBus.IO.SqlServer;
using ASureBus.Utils;

namespace ASureBus.IO.SagaPersistence;

internal class SagaSqlServerPersistenceService(ISqlServerService storage, IServiceProvider services) 
    : ISagaPersistenceService
{
    public async Task<object?> Get(SagaType sagaType, Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        var result = await storage.Get(sagaType.Type.Name, correlationId, cancellationToken)
            .ConfigureAwait(false);
        
        return string.IsNullOrWhiteSpace(result)
            ? null
            : Serializer.Deserialize(result, sagaType.Type, new SagaConverter(sagaType.Type, sagaType.SagaDataType, services));
    }

    public async Task Save<TItem>(TItem item, SagaType sagaType, Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        var serialized = Serializer.Serialize(item);
        
        await storage.Save(serialized, sagaType.Type.Name, correlationId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Delete(SagaType sagaType, Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        await storage.Delete(sagaType.Type.Name, correlationId, cancellationToken)
            .ConfigureAwait(false);
    }
}