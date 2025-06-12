using ASureBus.Abstractions;
using ASureBus.ConfigurationObjects;
using ASureBus.Core;
using ASureBus.Core.Caching;
using ASureBus.Core.Sagas;
using ASureBus.Core.TypesHandling.Entities;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.Sagas;

[TestFixture]
public class SagaBehaviourTests
{
    private Mock<IAsbCache> _mockCache;
    // ReSharper disable once InconsistentNaming
    private Mock<ISagaIO> _mockSagaIO;
    private SagaBehaviour _sagaBehaviour;

    [SetUp]
    public void SetUp()
    {
        AsbConfiguration.DataStorageSagaPersistence = new DataStorageSagaPersistenceConfig()
        {
            ConnectionString = "connectionString",
            Container = "container"
        };

        _mockCache = new Mock<IAsbCache>();
        _mockSagaIO = new Mock<ISagaIO>();
        _sagaBehaviour = new SagaBehaviour(_mockCache.Object, _mockSagaIO.Object);
    }

    [TearDown]
    public void TearDown()
    {
        AsbConfiguration.DataStorageSagaPersistence = null;
        AsbConfiguration.SqlServerSagaPersistence = null;
    }

    [Test]
    public void SetCorrelationId_SetsCorrelationIdProperty()
    {
        // Arrange
        var sagaType = new SagaType { Type = typeof(TestSaga) };
        var correlationId = Guid.NewGuid();
        var saga = new TestSaga();

        // Act
        _sagaBehaviour.SetCorrelationId(sagaType, correlationId, saga);

        // Assert
        Assert.That(saga.CorrelationId, Is.EqualTo(correlationId));
    }

    [Test]
    public void HandleCompletion_AddsEventHandler()
    {
        // Arrange
        var sagaType = new SagaType { Type = typeof(TestSaga) };
        var correlationId = Guid.NewGuid();
        var saga = new TestSaga { CorrelationId = correlationId };

        // Act
        _sagaBehaviour.HandleCompletion(sagaType, correlationId, saga);
        saga.Complete();

        // Assert
        _mockCache.Verify(c => c.Remove(correlationId), Times.Once);
    }

    private class TestSaga : ISaga
    {
        public Guid CorrelationId { get; set; }
        public event EventHandler<SagaCompletedEventArgs>? Completed;
        public bool IsComplete { get; }

        public void Complete()
        {
            Completed?.Invoke(this, new SagaCompletedEventArgs
            {
                CorrelationId = CorrelationId,
                Type = GetType()
            });
        }
    }
}