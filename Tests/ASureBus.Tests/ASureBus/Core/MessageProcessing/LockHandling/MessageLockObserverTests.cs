using ASureBus.ConfigurationObjects.Options;
using ASureBus.Core.MessageProcessing.LockHandling;
using Azure.Core.Amqp;
using Azure.Messaging.ServiceBus;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.MessageProcessing.LockHandling;

[TestFixture]
[Ignore("Time is tricky to test reliably - needs investigation")]
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

    [Test]
    public void RenewOnExpiration_WithMaxDuration_StopsRenewingAfterDurationExceeded()
    {
        // Arrange
        var lockToken = Guid.NewGuid();
        var lockTokenBytes = BinaryData.FromString(lockToken.ToString());

        var bytes = new List<ReadOnlyMemory<byte>> { new byte[] { 1, 2, 3 } };
        var messageBody = new AmqpMessageBody(bytes);
        var amqpMessage = new AmqpAnnotatedMessage(messageBody);

        // Set lock to expire in 50ms (will trigger renewals at 50ms intervals)
        amqpMessage.MessageAnnotations.Add("x-opt-locked-until", DateTime.UtcNow.AddMilliseconds(50));
        var msg = ServiceBusReceivedMessage.FromAmqpMessage(amqpMessage, lockTokenBytes);

        // Configure
        // No preemptive threshold, max duration of 90ms
        AsbConfiguration.MessageLockOptions = new MessageLockRenewalOptions
        {
            MessageLockRenewalPreemptiveThresholdInSeconds = 0,
            MaxAutoLockRenewalDuration = TimeSpan.FromMilliseconds(100)
        };

        var args = new ProcessMessageEventArgsMock(msg);

        // Act
        _lockObserver.RenewOnExpiration(args);
        Thread.Sleep(150); // Allow time for renewals

        // Assert
        // Should have renewed only once (at ~50ms), not at ~100ms (exceeds 90ms max)
        Assert.That(args.CallCountRenewMessageLockAsync, Is.EqualTo(1));
    }

    [Test]
    public void RenewOnExpiration_WithMaxDurationNotExceeded_ContinuesRenewing()
    {
        // Arrange
        var lockToken = Guid.NewGuid();
        var lockTokenBytes = BinaryData.FromString(lockToken.ToString());

        var bytes = new List<ReadOnlyMemory<byte>> { new byte[] { 1, 2, 3 } };
        var messageBody = new AmqpMessageBody(bytes);
        var amqpMessage = new AmqpAnnotatedMessage(messageBody);

        // Set lock to expire in 30ms
        amqpMessage.MessageAnnotations.Add("x-opt-locked-until", DateTime.UtcNow.AddMilliseconds(30));
        var msg = ServiceBusReceivedMessage.FromAmqpMessage(amqpMessage, lockTokenBytes);

        // Configure
        // Max duration of 200ms (plenty of time for multiple renewals)
        AsbConfiguration.MessageLockOptions = new MessageLockRenewalOptions
        {
            MessageLockRenewalPreemptiveThresholdInSeconds = 0,
            MaxAutoLockRenewalDuration = TimeSpan.FromMilliseconds(200)
        };

        var args = new ProcessMessageEventArgsMock(msg);

        // Act
        _lockObserver.RenewOnExpiration(args);
        Thread.Sleep(120);

        // Assert
        // Should have renewed multiple times (at ~30ms, ~60ms, ~90ms)
        Assert.That(args.CallCountRenewMessageLockAsync, Is.GreaterThanOrEqualTo(3));
    }

    [Test]
    public void RenewOnExpiration_WithoutMaxDuration_ContinuesRenewingIndefinitely()
    {
        // Arrange
        var lockToken = Guid.NewGuid();
        var lockTokenBytes = BinaryData.FromString(lockToken.ToString());

        var bytes = new List<ReadOnlyMemory<byte>> { new byte[] { 1, 2, 3 } };
        var messageBody = new AmqpMessageBody(bytes);
        var amqpMessage = new AmqpAnnotatedMessage(messageBody);

        amqpMessage.MessageAnnotations.Add("x-opt-locked-until", DateTime.UtcNow.AddMilliseconds(25));
        var msg = ServiceBusReceivedMessage.FromAmqpMessage(amqpMessage, lockTokenBytes);

        // Configure
        var veryLongDuration = TimeSpan.FromHours(1);

        AsbConfiguration.MessageLockOptions = new MessageLockRenewalOptions
        {
            MessageLockRenewalPreemptiveThresholdInSeconds = 0,
            MaxAutoLockRenewalDuration = veryLongDuration
        };

        var args = new ProcessMessageEventArgsMock(msg);

        // Act
        _lockObserver.RenewOnExpiration(args);
        Thread.Sleep(150);

        // Assert
        const int anyManyTimes = 5;
        Assert.That(args.CallCountRenewMessageLockAsync, Is.GreaterThanOrEqualTo(anyManyTimes));
    }
}

internal class ProcessMessageEventArgsMock(ServiceBusReceivedMessage message, int lockRenewalDurationMs = 50)
    : ProcessMessageEventArgs(message,
        It.IsAny<ServiceBusReceiver>(),
        It.IsAny<CancellationToken>())
{
    public int CallCountRenewMessageLockAsync { get; private set; }

    public override Task RenewMessageLockAsync(ServiceBusReceivedMessage message,
        CancellationToken cancellationToken = default)
    {
        CallCountRenewMessageLockAsync++;

        var newLockedUntil = DateTimeOffset.UtcNow.AddMilliseconds(lockRenewalDurationMs);
        CreateMessageWithNewLockTime(newLockedUntil);

        return Task.CompletedTask;
    }

    private ServiceBusReceivedMessage CreateMessageWithNewLockTime(DateTimeOffset newLockedUntil)
    {
        var lockTokenBytes = BinaryData.FromString(Guid.NewGuid().ToString());
        var bytes = new List<ReadOnlyMemory<byte>> { new byte[] { 1, 2, 3 } };
        var messageBody = new AmqpMessageBody(bytes);
        var amqpMessage = new AmqpAnnotatedMessage(messageBody);

        amqpMessage.MessageAnnotations.Add("x-opt-locked-until", newLockedUntil.DateTime);

        return ServiceBusReceivedMessage.FromAmqpMessage(amqpMessage, lockTokenBytes);
    }
}