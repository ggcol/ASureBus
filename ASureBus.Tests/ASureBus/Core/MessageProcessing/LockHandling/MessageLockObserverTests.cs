using ASureBus.ConfigurationObjects.Options;
using ASureBus.Core;
using ASureBus.Core.MessageProcessing.LockHandling;
using Azure.Core.Amqp;
using Azure.Messaging.ServiceBus;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.MessageProcessing.LockHandling;

[TestFixture]
public class MessageLockObserverTests
{
    private IMessageLockObserver _lockObserver;

    [SetUp]
    public void SetUp()
    {
        _lockObserver = new MessageLockObserver();
    }

    [TearDown]
    public void TearDown()
    {
        AsbConfiguration.MessageLockOptions = null!;
    }
    
    [Test]
    public void RenewOnExpiration_ShouldCallRenewMessageLockOnProcessMessageEventArgs()
    {
        // Arrange

        // Create a ServiceBusReceivedMessage
        var lockToken = Guid.NewGuid();
        var lockTokenBytes = BinaryData.FromString(lockToken.ToString());
        
        var bytes = new List<ReadOnlyMemory<byte>> { new byte[] { 1, 2, 3 } };
        var messageBody = new AmqpMessageBody(bytes);
        var amqpMessage = new AmqpAnnotatedMessage(messageBody);
        
        // Set LockedUntil annotation to 20 milliseconds,
        // this will propagate to the ServiceBusReceivedMessage LockedUntil property
        amqpMessage.MessageAnnotations.Add("x-opt-locked-until", DateTime.UtcNow.AddMilliseconds(20));
        var msg = ServiceBusReceivedMessage.FromAmqpMessage(amqpMessage, lockTokenBytes);
        
        AsbConfiguration.MessageLockOptions = new MessageLockRenewalOptions
            {
                MessageLockRenewalPreemptiveThresholdInSeconds = 0
            };
        
        var args = new ProcessMessageEventArgsMock(msg);
        
        // Act
        _lockObserver.RenewOnExpiration(args);
        Thread.Sleep(30);
        
        // Assert
        Assert.That(args.CallCountRenewMessageLockAsync, Is.EqualTo(1));
    }
}

internal class ProcessMessageEventArgsMock : ProcessMessageEventArgs
{
    public ProcessMessageEventArgsMock(ServiceBusReceivedMessage message)
        : base(
            message,
            It.IsAny<ServiceBusReceiver>(),
            It.IsAny<CancellationToken>())
    {
    }
    
    public int CallCountRenewMessageLockAsync { get; private set; } = 0;

    public ProcessMessageEventArgsMock(ServiceBusReceivedMessage message, ServiceBusReceiver receiver,
        CancellationToken cancellationToken) : base(message, receiver, cancellationToken)
    {
    }

    public ProcessMessageEventArgsMock(ServiceBusReceivedMessage message, ServiceBusReceiver receiver,
        string identifier, CancellationToken cancellationToken) : base(message, receiver, identifier, cancellationToken)
    {
    }
    
    public override Task RenewMessageLockAsync(ServiceBusReceivedMessage message,
        CancellationToken cancellationToken = default)
    {
        CallCountRenewMessageLockAsync++;
        return Task.CompletedTask;
    }
}
