using ASureBus.Abstractions;
using ASureBus.ConfigurationObjects.Config;
using ASureBus.Core;
using ASureBus.Core.TypesHandling.Entities;
using ASureBus.Services.SqlServer;
using ASureBus.Services.SqlServer.DbConnection;
using ASureBus.Utils;

namespace ASureBus.Tests.ASureBus.Services.SqlServer;

[TestFixture]
public class SqlServerServiceTests
{
    private SqlServerService _service;
    private IDbConnectionFactory _connectionFactory;

    [SetUp]
    public void SetUp()
    {
        AsbConfiguration.SqlServerSagaPersistence = new SqlServerSagaPersistenceConfig
        {
            ConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                               ??
                               "Server=(localdb)\\mssqllocaldb;Trusted_Connection=False;MultipleActiveResultSets=true",
            Schema = "sagas"
        };
        
        _connectionFactory = new InMemorySqlServerConnectionFactory();
        _service = new SqlServerService(_connectionFactory);
        
        DeleteTableForTesting();
        CreateTableForTesting();
    }
    
    [TearDown]
    public void TearDown()
    {
        DeleteTableForTesting();
        AsbConfiguration.SqlServerSagaPersistence = null;
    }

    private void CreateTableForTesting()
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
                CREATE TABLE SagaType (
                    CorrelationId UNIQUEIDENTIFIER PRIMARY KEY,
                    Saga NVARCHAR(MAX)
                )";
        cmd.ExecuteNonQuery();
    }

    private void DeleteTableForTesting()
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DROP TABLE IF EXISTS SagaType";
        cmd.ExecuteNonQuery();
    }

    [Test]
    public async Task SaveAndGetSaga_Success()
    {
        // Arrange
        var sagaType = MakeTestSagaType();
        var tableName = sagaType.Type.Name;
        var correlationId = Guid.NewGuid();
        var item = new PersistentSaga
        {
            CorrelationId = correlationId
        };
        var serializedItem = Serializer.Serialize(item);

        // Act
        await _service.Save(serializedItem, tableName, correlationId);
        var result = await _service.Get(tableName, correlationId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(serializedItem));
    }

    [Test]
    public async Task DeleteSaga_Success()
    {
        // Arrange
        var sagaType = MakeTestSagaType();
        var tableName = sagaType.Type.Name;
        var correlationId = Guid.NewGuid();
        var item = new PersistentSaga
        {
            CorrelationId = correlationId
        };

        var serializedItem = Serializer.Serialize(item);

        // Act
        await _service.Save(serializedItem, tableName, correlationId);
        await _service.Delete(tableName, correlationId);

        var result = await _service.Get(tableName, correlationId);
        
        // Assert
        Assert.That(result, Is.Null);
    }

    private static SagaType MakeTestSagaType()
    {
        return new SagaType
        {
            Type = typeof(PersistentSaga),
            SagaDataType = typeof(PersistentSagaData),
            Listeners =
            [
                new SagaHandlerType()
                {
                    IsInitMessageHandler = true,
                    IsTimeoutHandler = false,
                    MessageType = new MessageType()
                    {
                        IsCommand = true,
                        Type = typeof(PersistentSagaCommand)
                    }
                }
            ]
        };
    }

    private class PersistentSaga : Saga<PersistentSagaData>, IAmStartedBy<PersistentSagaCommand>
    {
        public Task Handle(PersistentSagaCommand message, IMessagingContext context,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private class PersistentSagaData : SagaData
    {
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record PersistentSagaCommand : IAmACommand;
}