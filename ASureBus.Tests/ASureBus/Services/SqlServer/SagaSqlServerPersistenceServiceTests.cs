using ASureBus.Core.TypesHandling.Entities;
using ASureBus.Services.SqlServer;
using Moq;
using ASureBus.Abstractions;
using ASureBus.Utils;

namespace ASureBus.Tests.ASureBus.Services.SqlServer;

[TestFixture]
public class SagaSqlServerPersistenceServiceTests
{
    private Mock<ISqlServerService> _mockStorage;
    private Mock<IServiceProvider> _mockServiceProvider;
    private SagaSqlServerPersistenceService _service;

    [SetUp]
    public void SetUp()
    {
        _mockStorage = new Mock<ISqlServerService>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _service = new SagaSqlServerPersistenceService(_mockStorage.Object, _mockServiceProvider.Object);
    }

    [Test]
    public async Task Get_ShouldReturnObject_WhenSagaExists()
    {
        // Arrange
        var sagaType = MakeTestSagaType();
        var correlationId = Guid.NewGuid();
        var aSagaInstance = new ASaga()
        {
            CorrelationId = correlationId
        };

        var serializedObject = Serializer.Serialize(aSagaInstance);

        _mockStorage
            .Setup(s => s.Get(sagaType.Type.Name, correlationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serializedObject);

        // Act
        var result = await _service.Get(sagaType, correlationId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.AssignableFrom<ASaga>());
            Assert.That(((result as ASaga)!).CorrelationId,
                Is.EqualTo(aSagaInstance.CorrelationId));
        });
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public async Task Get_ShouldReturnNull_WhenSagaDoesNotExist(string? invalidRetrievedStrings)
    {
        // Arrange
        var sagaType = MakeTestSagaType();
        var correlationId = Guid.NewGuid();

        _mockStorage.Setup(s =>
                s.Get(sagaType.Type.Name, correlationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _service.Get(sagaType, correlationId);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Save_ShouldCallStorageSave()
    {
        // Arrange
        var sagaType = MakeTestSagaType();
        var correlationId = Guid.NewGuid();
        var item = new ASaga()
        {
            CorrelationId = correlationId
        };
        var serializedItem = Serializer.Serialize(item);

        // Act
        await _service.Save(item, sagaType, correlationId);

        // Assert
        _mockStorage.Verify(
            s => s.Save(serializedItem, sagaType.Type.Name, correlationId,
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Delete_ShouldCallStorageDelete()
    {
        // Arrange
        var sagaType = MakeTestSagaType();
        var correlationId = Guid.NewGuid();

        // Act
        await _service.Delete(sagaType, correlationId);

        // Assert
        _mockStorage.Verify(
            s => s.Delete(sagaType.Type.Name, correlationId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static SagaType MakeTestSagaType()
    {
        return new SagaType
        {
            Type = typeof(ASaga),
            SagaDataType = typeof(ASagaData),
            Listeners =
            [
                new SagaHandlerType()
                {
                    IsInitMessageHandler = true,
                    MessageType = new MessageType()
                    {
                        IsCommand = true,
                        Type = typeof(ASagaDataCommand)
                    }
                }
            ]
        };
    }

    private class ASaga : Saga<ASagaData>, IAmStartedBy<ASagaDataCommand>
    {
        public Task Handle(ASagaDataCommand message, IMessagingContext context,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private class ASagaData : SagaData
    {
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record ASagaDataCommand : IAmACommand;
}