using ASureBus.Abstractions;
using ASureBus.Core.Caching;
using ASureBus.Core.Sagas;
using ASureBus.Core.TypesHandling.Entities;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.Sagas;

[TestFixture]
public class SagaFactoryTests
{
    private IAsbCache _cache;
    
    [SetUp]
    public void SetUp()
    {
        _cache = new AsbCache();
    }

    [Test]
    public async Task Get_ReturnsSagaFromCache_WhenSagaExistsInCache()
    {
        //Arrange
        var correlationId = Guid.NewGuid();
        var saga = new TestSaga();
        
        _cache.Set(correlationId, saga);

        var sagaType = new SagaType();
        var listenerType = new SagaHandlerType()
        {
            IsInitMessageHandler = false,
            IsTimeoutHandler = false
        };
        
        var factory = new SagaFactory(_cache, Mock.Of<ISagaIO>(), Mock.Of<IServiceProvider>());
        
        //Act
        var result = await factory.Get(sagaType, listenerType, correlationId, CancellationToken.None);
        
        //Assert
        Assert.That(result, Is.InstanceOf<TestSaga>());
        Assert.That(result, Is.EqualTo(saga));
    }

    [Test]
    public async Task Get_ReturnsSagaFromIo_WhenSagaExistsInIo()
    {
        //Arrange
        var correlationId = Guid.NewGuid();
        var saga = new TestSaga();
        var sagaType = new SagaType
        {
            Type = typeof(TestSaga)
        };
        var listenerType = new SagaHandlerType
        {
            IsInitMessageHandler = false,
            IsTimeoutHandler = false
        };
        var sagaIoMock = new Mock<ISagaIO>();
        
        sagaIoMock
            .Setup(io => io.Load(correlationId, sagaType, CancellationToken.None))
            .ReturnsAsync(saga);
        
        var factory = new SagaFactory(_cache, sagaIoMock.Object, Mock.Of<IServiceProvider>());
        
        //Act
        var result = await factory.Get(sagaType, listenerType, correlationId, CancellationToken.None);
        
        //Assert
        Assert.That(result, Is.InstanceOf<TestSaga>());
        Assert.That(result, Is.EqualTo(saga));
        sagaIoMock.Verify(io => io.Load(correlationId, sagaType, CancellationToken.None), Times.Once);
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