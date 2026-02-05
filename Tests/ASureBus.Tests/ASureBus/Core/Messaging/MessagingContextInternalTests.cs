using ASureBus.Abstractions;
using ASureBus.Abstractions.Options.Messaging;
using ASureBus.Core.Messaging;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.Messaging;

[TestFixture]
public class MessagingContextInternalTests
{
    private MessagingContextInternal _messagingContext;

    [SetUp]
    public void SetUp()
    {
        _messagingContext = new MessagingContextInternal();
    }
    
    [Test]
    public async Task Send_ShouldEnqueueMessage()
    {
        // Arrange
        var message = new Mock<IAmACommand>();

        // Act
        await _messagingContext.Send(message.Object);

        // Assert
        Assert.That(_messagingContext.Messages, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Send_WithOptions_ShouldEnqueueMessage()
    {
        // Arrange
        var message = new Mock<IAmACommand>();
        var options = new SendOptions();
        

        // Act
        await _messagingContext.Send(message.Object, options);

        // Assert
        Assert.That(_messagingContext.Messages, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Send_WithOptions_ShouldEnqueueScheduledMessage()
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
        Assert.That(_messagingContext.Messages, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task SendAfter_ShouldEnqueueScheduledMessage()
    {
        // Arrange
        var message = new Mock<IAmACommand>();
        var delay = TimeSpan.FromMinutes(10);
        

        // Act
        await _messagingContext.SendAfter(message.Object, delay);

        // Assert
        Assert.That(_messagingContext.Messages, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Publish_ShouldEnqueueMessage()
    {
        // Arrange
        var message = new Mock<IAmAnEvent>();
        

        // Act
        await _messagingContext.Publish(message.Object);

        // Assert
        Assert.That(_messagingContext.Messages, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Publish_WithOptions_ShouldEnqueueMessage()
    {
        // Arrange
        var message = new Mock<IAmAnEvent>();
        var options = new PublishOptions();
        

        // Act
        await _messagingContext.Publish(message.Object, options);

        // Assert
        Assert.That(_messagingContext.Messages, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Publish_WithOptions_ShouldEnqueueScheduledMessage()
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
        Assert.That(_messagingContext.Messages, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task PublishAfter_ShouldEnqueueScheduledMessage()
    {
        // Arrange
        var message = new Mock<IAmAnEvent>();
        var delay = TimeSpan.FromMinutes(10);
        

        // Act
        await _messagingContext.PublishAfter(message.Object, delay);

        // Assert
        Assert.That(_messagingContext.Messages, Has.Count.EqualTo(1));
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