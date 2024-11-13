﻿using System.Text;
using ASureBus.Abstractions;
using ASureBus.Accessories.Heavy;
using ASureBus.Core.Enablers;
using ASureBus.Core.Entities;
using ASureBus.Utils;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.Enablers;

[TestFixture]
public class HandlerBrokerTests
{
    private FakeHandler _fakeHandler;
    private Mock<IMessagingContext> _mockContext;
    private HandlerBroker<HandlerBrokerTestsMessage> _handlerBroker;
    
    [SetUp]
    public void SetUp()
    {
        _fakeHandler = new FakeHandler();
        _mockContext = new Mock<IMessagingContext>();

        _handlerBroker = new HandlerBroker<HandlerBrokerTestsMessage>(_fakeHandler,
            _mockContext.Object);
    }

    [Test]
    public async Task Handle_ShouldCallHandlerWithDeserializedMessage()
    {
        // Arrange
        var cancellationToken = It.IsAny<CancellationToken>();

        var testMessage = new HandlerBrokerTestsMessage
        {
            AProperty = "TestValue"
        };

        var asbMessage = new AsbMessage<HandlerBrokerTestsMessage>
        {
            Message = testMessage,
            Heavies = new List<HeavyReference>(),
            MessageId = Guid.NewGuid()
        };

        var json = Serializer.Serialize(asbMessage);
        var binaryData = new BinaryData(Encoding.UTF8.GetBytes(json));

        // Act
        await _handlerBroker.Handle(binaryData, cancellationToken);

        // Assert
        Assert.That(_fakeHandler.HandleCallCount, Is.EqualTo(1));
    }

    [Test]
    public async Task HandleError_ShouldCallHandlerWithException()
    {
        // Arrange
        var exception = new Exception("Test exception");

        // Act
        await _handlerBroker.HandleError(exception);

        // Assert
        Assert.That(_fakeHandler.HandleErrorCallCount, Is.EqualTo(1));
    }
}

internal class HandlerBrokerTestsMessage : IAmAMessage
{
    public string? AProperty { get; set; }
}

internal class FakeHandler : IHandleMessage<HandlerBrokerTestsMessage>
{
    public int HandleCallCount { get; private set; }
    public int HandleErrorCallCount { get; private set; }

    public Task Handle(HandlerBrokerTestsMessage message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        HandleCallCount++;
        return Task.CompletedTask;
    }

    public Task HandleError(Exception ex, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        HandleErrorCallCount++;
        return Task.CompletedTask;
    }
}