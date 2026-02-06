using System.Text;
using ASureBus.Abstractions;
using ASureBus.Accessories.Heavies.Entities;
using ASureBus.Core.Enablers;
using ASureBus.Core.Entities;
using ASureBus.Utils;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.Enablers;

[TestFixture]
public class SagaBrokerTests
{
    private FakeSaga _fakeSaga;
    private Mock<IMessagingContext> _contextMock;
    private SagaBroker<FakeSagaData, SagaBrokerTestMessage> _sagaBroker;

    [SetUp]
    public void SetUp()
    {
        _fakeSaga = new FakeSaga();
        _contextMock = new Mock<IMessagingContext>();
        _sagaBroker = new SagaBroker<FakeSagaData, SagaBrokerTestMessage>(_fakeSaga, _contextMock.Object);
    }

    [Test]
    public async Task Handle_ShouldInvokeSagaHandleMethod()
    {
        // Arrange
        var cancellationToken = It.IsAny<CancellationToken>();
        var testMessage = new SagaBrokerTestMessage();
        
        var asbMessage = new AsbMessage<SagaBrokerTestMessage>
        {
            Header = new AsbMessageHeader()
            {
                Heavies = new List<HeavyReference>(),
                MessageId = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid(),
                MessageName = nameof(SagaBrokerTestMessage),
                Destination = nameof(SagaBrokerTestMessage),
                IsCommand = testMessage is IAmACommand
            },
            Message = testMessage
        };
        
        var json = Serializer.Serialize(asbMessage);
        var binaryData = new BinaryData(Encoding.UTF8.GetBytes(json));
        
        // Act
        await _sagaBroker.Handle(binaryData, cancellationToken);

        // Assert
        Assert.That(_fakeSaga.HandleCallCount, Is.EqualTo(1));
    }

    [Test]
    public async Task HandleError_ShouldInvokeSagaHandleErrorMethod()
    {
        // Arrange
        var exception = new Exception("test exception");
        
        // Act
        await _sagaBroker.HandleError(exception);

        // Assert
        Assert.That(_fakeSaga.HandleErrorCallCount, Is.EqualTo(1));
    }

    private class FakeSaga : Saga<FakeSagaData>, IAmStartedBy<SagaBrokerTestMessage>
    {
        public int HandleCallCount { get; private set; }
        public int HandleErrorCallCount { get; private set; } 
        
        public Task Handle(SagaBrokerTestMessage message, IMessagingContext context,
            CancellationToken cancellationToken = default)
        {
            HandleCallCount++;
            return Task.FromResult(Task.CompletedTask);
        }
        
        public Task HandleError(Exception ex, IMessagingContext context,
            CancellationToken cancellationToken = default)
        {
            HandleErrorCallCount++;
            return Task.FromResult(Task.CompletedTask);
        }
    }
    
    private class SagaBrokerTestMessage : IAmACommand { }

    private class FakeSagaData : SagaData { }
}