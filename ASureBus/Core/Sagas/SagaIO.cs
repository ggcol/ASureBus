using ASureBus.Core.TypesHandling.Entities;
using ASureBus.IO.SagaPersistence;
using ASureBus.IO.SqlServer;
using ASureBus.IO.SqlServer.DbConnection;
using ASureBus.IO.StorageAccount;

namespace ASureBus.Core.Sagas;

// ReSharper disable once InconsistentNaming
internal class SagaIO : ISagaIO
{
    private readonly ISagaPersistenceService? _persistenceService;
    private readonly bool _isInUse = AsbConfiguration.OffloadSagas;

    public SagaIO(IServiceProvider services)
    {
        if (_isInUse)
        {
            _persistenceService = MakeStorageService(services);
        }
    }

    private ISagaPersistenceService? MakeStorageService(IServiceProvider services)
    {
        if (AsbConfiguration.UseDataStorageSagaPersistence)
        {
            return new SagaDataStoragePersistenceService(
                new AzureDataStorageService(
                    AsbConfiguration.DataStorageSagaPersistence?.ConnectionString)
                , services
            );
        }

        if (AsbConfiguration.UseSqlServerSagaPersistence)
        {
            return new SagaSqlServerPersistenceService(
                new SqlServerService(
                    new SqlServerConnectionFactory(AsbConfiguration.SqlServerSagaPersistence!
                        .ConnectionString)
                )
                , services
            );
        }

        throw new NotImplementedException(); //TODO customize
    }

    public async Task<object?> Load(Guid correlationId, SagaType sagaType,
        CancellationToken cancellationToken = default)
    {
        if (!_isInUse) return null;

        return await _persistenceService!.Get(sagaType, correlationId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Unload(object? implSaga, Guid correlationId,
        SagaType sagaType, CancellationToken cancellationToken = default)
    {
        if (!_isInUse) return;

        await _persistenceService!.Save(implSaga, sagaType, correlationId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Delete(Guid correlationId, SagaType sagaType,
        CancellationToken cancellationToken = default)
    {
        if (!_isInUse) return;

        await _persistenceService!.Delete(sagaType, correlationId, cancellationToken)
            .ConfigureAwait(false);
    }
}