using ASureBus.Abstractions;

namespace ASureBus.Tests.Abstractions;

public class TestSagaData : SagaData
{
}

public class TestSaga : Saga<TestSagaData>
{
    public void CompleteSaga()
    {
        IAmComplete();
    }
}

[TestFixture]
public class SagaTests
{
    private TestSaga _saga;

    [SetUp]
    public void SetUp()
    {
        _saga = new TestSaga();
    }

    [Test]
    public void Constructor_InitializesWithNewSagaData()
    {
        // Assert
        Assert.That(_saga.SagaData, Is.Not.Null);
        Assert.That(_saga.SagaData, Is.TypeOf<TestSagaData>());
    }
    
    //note that this tested property is internal only
    [Test]
    public void CorrelationId_SetAndGet_ReturnsCorrectValue()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        _saga.CorrelationId = correlationId;

        // Act
        var result = _saga.CorrelationId;

        // Assert
        Assert.That(result, Is.EqualTo(correlationId));
    }
    
    //note that the event tested is internal only
    [Test]
    public void IAmComplete_RaisesCompletedEvent()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        _saga.CorrelationId = correlationId;
        SagaCompletedEventArgs? eventArgs = null;

        _saga.Completed += (_, args) =>
        {
            eventArgs = args;
        };

        // Act
        _saga.CompleteSaga();

        // Assert
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs!.CorrelationId, Is.EqualTo(correlationId));
    }
    
    //note that the event tested is internal only
    [Test]
    public void IAmComplete_NoSubscribers_DoesNotThrowException()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        _saga.CorrelationId = correlationId;

        // Act & Assert
        Assert.DoesNotThrow(() => _saga.CompleteSaga());
    }
}