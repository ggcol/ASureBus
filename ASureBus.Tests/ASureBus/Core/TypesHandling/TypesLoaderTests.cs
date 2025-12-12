using System.Reflection;
using ASureBus.Abstractions;
using ASureBus.Core.TypesHandling;
using ASureBus.Core.TypesHandling.Entities;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.TypesHandling;

[TestFixture]
public class TypesLoaderTests
{
    private Mock<Assembly> _mockAssembly;
    private TypesLoader _loader;

    [SetUp]
    public void SetUp()
    {
        _mockAssembly = new Mock<Assembly>();
        _loader = new TypesLoader(_mockAssembly.Object);
    }

    [Test]
    public void Constructor_WhenCalled_InitializesHandlersAndSagas()
    {
        // Act
        var loader = new TypesLoader(_mockAssembly.Object);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(loader.Handlers, Is.Not.Null);
            Assert.That(loader.Sagas, Is.Not.Null);
        });
    }

    [Test]
    public void GetHandlers_WhenCalled_ReturnsExpectedHandlers()
    {
        // Arrange
        var handlerType = typeof(TestHandler);
        _mockAssembly.Setup(a => a.GetTypes())
            .Returns(new[] { handlerType });

        // Act
        var handlers = _loader.GetType()
            .GetMethod("GetHandlers", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(_loader, new object[] { _mockAssembly.Object }) as IEnumerable<HandlerType>;

        // Assert
        Assert.That(handlers, Is.Not.Null);
        Assert.That(handlers.Any(h => h.Type == handlerType), Is.True);
    }

    [Test]
    public void GetSagas_WhenCalled_ReturnsExpectedSagas()
    {
        // Arrange
        var sagaType = typeof(TestSaga);
        _mockAssembly.Setup(a => a.GetTypes())
            .Returns(new[] { sagaType });

        // Act
        var sagas = _loader.GetType()
            .GetMethod("GetSagas", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(_loader, new object[] { _mockAssembly.Object }) as IEnumerable<SagaType>;

        // Assert
        Assert.That(sagas, Is.Not.Null);
        Assert.That(sagas.Any(s => s.Type == sagaType), Is.True);
    }

    [Test]
    public void GetHandlers_ShouldReturnEmpty_WhenAssemblyIsNull()
    {
        // Act
        var handlers = _loader.GetType()
            .GetMethod("GetHandlers", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(_loader, new object[] { null }) as IEnumerable<HandlerType>;

        // Assert
        Assert.That(handlers, Is.Not.Null);
        Assert.That(handlers, Is.Empty);
    }

    [Test]
    public void GetSagas_ShouldReturnEmpty_WhenAssemblyIsNull()
    {
        // Act
        var sagas = _loader.GetType()
            .GetMethod("GetSagas", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(_loader, new object[] { null }) as IEnumerable<SagaType>;

        // Assert
        Assert.That(sagas, Is.Not.Null);
        Assert.That(sagas, Is.Empty);
    }

    private class TestHandler : IHandleMessage<TestMessage>
    {
        public Task Handle(TestMessage message, IMessagingContext context,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private class TestSaga : Saga<TestSagaData>, IAmStartedBy<TestMessage>
    {
        public Task Handle(TestMessage message, IMessagingContext context,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class TestMessage : IAmACommand
    {
    }

    private class TestSagaData : SagaData
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
    }
}