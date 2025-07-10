using ASureBus.Abstractions;
using ASureBus.ConfigurationObjects;
using ASureBus.Core;
using ASureBus.Core.Entities;
using ASureBus.Core.MessageProcessing;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.MessageProcessing;

internal class MockProcessor : MessageProcessor
{
    public new async Task<AsbMessageHeader?> GetMessageHeader(BinaryData messageBody,
        CancellationToken cancellationToken)
    {
        return await base.GetMessageHeader(messageBody, cancellationToken).ConfigureAwait(false);
    }

    public new bool UsesHeavies(IAsbMessage asbMessage)
    {
        return base.UsesHeavies(asbMessage);
    }
}

internal record MockMessage : IAmAMessage;

public class MessageProcessorTests
{
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
        var processor = new MockProcessor();

        // Act
        var result = await processor.GetMessageHeader(validMessageBody, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.That(result.MessageId, Is.EqualTo(messageId));
    }

    [Test]
    public void UseHeavies_ReturnsFalse_WhenHeaviesAreConfiguredButNoHeaviesInMessage()
    {
        // Arrange
        AsbConfiguration.HeavyProps = new HeavyPropertiesConfig()
        {
            ConnectionString = "",
            Container = ""
        };

        var asbMessage = new AsbMessage<MockMessage>()
        {
            Message = new MockMessage(),
            Header = new AsbMessageHeader()
            {
                CorrelationId = It.IsAny<Guid>(),
                MessageId = It.IsAny<Guid>(),
                Destination = It.IsAny<string>(),
                MessageName = It.IsAny<string>(),
                IsCommand = It.IsAny<bool>(),
            }
        };

        var processor = new MockProcessor();

        // Act
        var result = processor.UsesHeavies(asbMessage);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void UseHeavies_ReturnsFalse_WhenHeaviesAreNotConfigured()
    {
        // Arrange
        AsbConfiguration.HeavyProps = null;

        var asbMessage = new AsbMessage<MockMessage>()
        {
            Message = new MockMessage(),
            Header = new AsbMessageHeader()
            {
                CorrelationId = It.IsAny<Guid>(),
                MessageId = It.IsAny<Guid>(),
                Destination = It.IsAny<string>(),
                MessageName = It.IsAny<string>(),
                IsCommand = It.IsAny<bool>(),
                Heavies = [],
            }
        };

        var processor = new MockProcessor();

        // Act
        var result = processor.UsesHeavies(asbMessage);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void UseHeavies_ReturnsTrue_WhenHeaviesAreConfiguredAndMessageHasAtLeastOne()
    {
        // Arrange
        AsbConfiguration.HeavyProps = new HeavyPropertiesConfig()
        {
            ConnectionString = "",
            Container = ""
        };;

        var asbMessage = new AsbMessage<MockMessage>()
        {
            Message = new MockMessage(),
            Header = new AsbMessageHeader()
            {
                CorrelationId = It.IsAny<Guid>(),
                MessageId = It.IsAny<Guid>(),
                Destination = It.IsAny<string>(),
                MessageName = It.IsAny<string>(),
                IsCommand = It.IsAny<bool>(),
                Heavies =
                [
                    new()
                    {
                    }
                ],
            }
        };

        var processor = new MockProcessor();

        // Act
        var result = processor.UsesHeavies(asbMessage);

        // Assert
        Assert.That(result, Is.True);
    }

    [TearDown]
    public void TearDown()
    {
        AsbConfiguration.HeavyProps = null;
    }
}