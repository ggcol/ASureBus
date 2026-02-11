using ASureBus.Abstractions;
using ASureBus.Abstractions.Options.Messaging;
using ASureBus.Core.Entities;
using ASureBus.Core.Messaging;
using ASureBus.IO.Heavies;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.Messaging;

[TestFixture]
public class CollectMessageTests
{
    private Mock<IAmAMessage> _mockMessage;
    private Mock<EmitOptions> _mockOptions;
    
    private TestCollectMessage _collectMessage;

    [SetUp]
    public void SetUp()
    {
        _mockMessage = new Mock<IAmAMessage>();
        _mockOptions = new Mock<EmitOptions>();

        // this grant no heavy properties related logic will be used
        AsbConfiguration.HeavyProps = null;

        _collectMessage = new TestCollectMessage(new Mock<IHeavyIO>().Object);
    }

    [Test]
    public async Task Enqueue_ShouldAddMessageToQueue()
    {
        // Act
        await _collectMessage.Enqueue(_mockMessage.Object, _mockOptions.Object,
            CancellationToken.None);

        // Assert
        Assert.That(_collectMessage.Messages, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Enqueue_ShouldSetScheduledTime()
    {
        // Arrange
        var scheduledTime = DateTimeOffset.UtcNow.AddMinutes(10);

        // Act
        await _collectMessage.Enqueue(_mockMessage.Object, scheduledTime, _mockOptions.Object,
            CancellationToken.None);
        var enqueuedMessage = (AsbMessage<IAmAMessage>)_collectMessage.Messages.Peek();

        // Assert
        Assert.That(enqueuedMessage.Header.ScheduledTime, Is.EqualTo(scheduledTime));
    }

    private class TestCollectMessage(IHeavyIO heavyIO) : CollectMessage(heavyIO)
    {
        public new async Task Enqueue<TMessage>(TMessage message, EmitOptions? options,
            CancellationToken cancellationToken)
            where TMessage : IAmAMessage
        {
            await base.Enqueue(message, options, cancellationToken).ConfigureAwait(false);
        }

        public new async Task Enqueue<TMessage>(TMessage message, DateTimeOffset? scheduledTime,
            EmitOptions? options, CancellationToken cancellationToken)
            where TMessage : IAmAMessage
        {
            await base.Enqueue(message, scheduledTime, options, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}