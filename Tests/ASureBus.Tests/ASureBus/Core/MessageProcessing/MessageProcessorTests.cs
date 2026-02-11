using ASureBus.Abstractions;
using ASureBus.Core.Entities;
using ASureBus.Core.MessageProcessing;
using Microsoft.Extensions.Logging;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.MessageProcessing;

internal class TestProcessor(ILogger logger) : MessageProcessor(logger)
{
    public new async Task<AsbMessageHeader?> GetMessageHeader(BinaryData messageBody,
        CancellationToken cancellationToken)
    {
        return await base.GetMessageHeader(messageBody, cancellationToken).ConfigureAwait(false);
    }
}

internal record MockMessage : IAmAMessage;

[TestFixture]
public class MessageProcessorTests
{
    private TestProcessor _processor;

    [SetUp]
    public void SetUp()
    {
        _processor = new TestProcessor(Mock.Of<ILogger>());
    }
    
    [Test]
    public async Task GetMessageHeader_ReturnsHeader_WhenMessageBodyIsValid()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var message = new AsbMessage<MockMessage>()
        {
            Header = new AsbMessageHeader()
            {
                CorrelationId = Guid.NewGuid(),
                MessageId = messageId,
                MessageName = "TestMessage",
                Destination = "TestDestination",
                IsCommand = true,
            },
            Message = new MockMessage()
        };
        var validMessageBody = new BinaryData(System.Text.Json.JsonSerializer.Serialize(message));
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _processor.GetMessageHeader(validMessageBody, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MessageId, Is.EqualTo(messageId));
    }

    [Test]
    public void IsMaxDeliveryCountExceeded_ReturnsTrue_WhenActualCountExceedsMaxRetries()
    {
        // Arrange
        var maxRetries = AsbConfiguration.ServiceBus.ClientOptions.RetryOptions.MaxRetries;
        var actualCount = maxRetries + 1;

        // Act
        var result = MessageProcessor.IsMaxDeliveryCountExceeded(actualCount);

        // Assert
        Assert.That(result, Is.True);
    }

    [TearDown]
    public void TearDown()
    {
        AsbConfiguration.HeavyProps = null;
    }
}