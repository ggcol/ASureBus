using ASureBus.Abstractions;
using ASureBus.Abstractions.Options.Messaging;
using ASureBus.Core.Messaging;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.Messaging;

[TestFixture]
public class MessagingContextTests
{
    private Mock<IMessageEmitter> _mockEmitter;
    private MessagingContext _messagingContext;

    [SetUp]
    public void SetUp()
    {
        _mockEmitter = new Mock<IMessageEmitter>();
        _messagingContext = new MessagingContext(_mockEmitter.Object);
    }

    [Test]
    public async Task Send_ShouldEnqueueAndFlushMessage()
    {
        // Arrange
        var message = new Mock<IAmACommand>();

        // Act
        await _messagingContext.Send(message.Object);

        // Assert
        _mockEmitter.Verify(
            e => e.FlushAll(It.IsAny<ICollectMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Send_WithOptions_ShouldEnqueueAndFlushMessage()
    {
        // Arrange
        var message = new Mock<IAmACommand>();
        var options = new SendOptions();

        // Act
        await _messagingContext.Send(message.Object, options);

        // Assert
        _mockEmitter.Verify(
            e => e.FlushAll(It.IsAny<ICollectMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Send_WithOptions_ShouldEnqueueAndFlushScheduledMessage()
    {
        // Arrange
        var message = new Mock<IAmACommand>();
        var options = new SendOptions
        {
            ScheduledTime = DateTimeOffset.UtcNow.AddSeconds(10)
        };

        // Act
        await _messagingContext.Send(message.Object, options);

        // Assert
        _mockEmitter.Verify(
            e => e.FlushAll(It.IsAny<ICollectMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task SendAfter_ShouldEnqueueAndFlushScheduledMessage()
    {
        // Arrange
        var message = new Mock<IAmACommand>();
        var delay = TimeSpan.FromMinutes(10);

        // Act
        await _messagingContext.SendAfter(message.Object, delay);

        // Assert
        _mockEmitter.Verify(
            e => e.FlushAll(It.IsAny<ICollectMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Publish_ShouldEnqueueAndFlushMessage()
    {
        // Arrange
        var message = new Mock<IAmAnEvent>();

        // Act
        await _messagingContext.Publish(message.Object);

        // Assert
        _mockEmitter.Verify(
            e => e.FlushAll(It.IsAny<ICollectMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Publish_WithOptions_ShouldEnqueueAndFlushMessage()
    {
        // Arrange
        var message = new Mock<IAmAnEvent>();
        var options = new PublishOptions();

        // Act
        await _messagingContext.Publish(message.Object, options);

        // Assert
        _mockEmitter.Verify(
            e => e.FlushAll(It.IsAny<ICollectMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Publish_WithOptions_ShouldEnqueueAndFlushScheduledMessage()
    {
        // Arrange
        var message = new Mock<IAmAnEvent>();
        var options = new PublishOptions
        {
            ScheduledTime = DateTimeOffset.UtcNow.AddSeconds(10)
        };

        // Act
        await _messagingContext.Publish(message.Object, options);

        // Assert
        _mockEmitter.Verify(
            e => e.FlushAll(It.IsAny<ICollectMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task PublishAfter_ShouldEnqueueAndFlushScheduledMessage()
    {
        // Arrange
        var message = new Mock<IAmAnEvent>();
        var delay = TimeSpan.FromMinutes(10);

        // Act
        await _messagingContext.PublishAfter(message.Object, delay);

        // Assert
        _mockEmitter.Verify(
            e => e.FlushAll(It.IsAny<ICollectMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void Bind_ShouldSetCorrelationId()
    {
        //Arrange
        var correlationId = Guid.NewGuid();
        
        //Act
        _messagingContext.Bind(correlationId);
        
        //Assert
        Assert.That(_messagingContext.CorrelationId, Is.EqualTo(correlationId));
    }
}