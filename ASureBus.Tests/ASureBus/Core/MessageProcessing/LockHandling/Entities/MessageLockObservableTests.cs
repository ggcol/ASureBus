using ASureBus.Core;
using ASureBus.Core.MessageProcessing.LockHandling.Entities;
using ASureBus.ConfigurationObjects.Options;
using Azure.Core.Amqp;
using Azure.Messaging.ServiceBus;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.MessageProcessing.LockHandling.Entities;

[TestFixture]
public class MessageLockObservableTests
{
    [SetUp]
    public void SetUp()
    {
        AsbConfiguration.MessageLockOptions = new MessageLockRenewalOptions
        {
            MessageLockRenewalPreemptiveThresholdInSeconds = 10
        };
    }

    [TearDown]
    public void TearDown()
    {
        AsbConfiguration.MessageLockOptions = null!;
    }

    [Test]
    public void Constructor_WithoutStartTime_SetsStartTimeToNow()
    {
        // Arrange
        var args = CreateMockArgs(lockedUntil: DateTimeOffset.UtcNow.AddSeconds(30));
        var beforeCreation = DateTimeOffset.UtcNow;

        // Act
        var observable = new MessageLockObservable(args);
        var afterCreation = DateTimeOffset.UtcNow;

        // Assert
        Assert.That(observable.StartTime, Is.Not.Null);
        Assert.That(observable.StartTime!.Value, Is.GreaterThanOrEqualTo(beforeCreation));
        Assert.That(observable.StartTime!.Value, Is.LessThanOrEqualTo(afterCreation));
    }

    [Test]
    public void Constructor_WithStartTime_PreservesStartTime()
    {
        // Arrange
        var originalStartTime = DateTimeOffset.UtcNow.AddMinutes(-5);
        var args = CreateMockArgs(lockedUntil: DateTimeOffset.UtcNow.AddSeconds(30));

        // Act
        var observable = new MessageLockObservable(args, originalStartTime);

        // Assert
        Assert.That(observable.StartTime, Is.EqualTo(originalStartTime));
    }

    [Test]
    public void Constructor_CalculatesExpirationTime_BasedOnLockedUntilAndThreshold()
    {
        // Arrange
        var lockedUntil = DateTimeOffset.UtcNow.AddSeconds(30);
        const int thresholdInSeconds = 10;
        AsbConfiguration.MessageLockOptions.MessageLockRenewalPreemptiveThresholdInSeconds = thresholdInSeconds;

        var args = CreateMockArgs(lockedUntil: lockedUntil);

        // Act
        var observable = new MessageLockObservable(args);

        // Assert
        Assert.That(observable.HasExpiration, Is.True);
    }

    [Test]
    public void MultipleInstances_WithSameStartTime_MaintainConsistentStartTime()
    {
        // Arrange
        var originalStartTime = DateTimeOffset.UtcNow.AddMinutes(-2);
        var args1 = CreateMockArgs(lockedUntil: DateTimeOffset.UtcNow.AddSeconds(30));
        var args2 = CreateMockArgs(lockedUntil: DateTimeOffset.UtcNow.AddSeconds(30));

        var observable1 = new MessageLockObservable(args1, originalStartTime);
        Thread.Sleep(50);

        var observable2 = new MessageLockObservable(args2, observable1.StartTime);
        Thread.Sleep(50);

        var observable3 = new MessageLockObservable(args2, observable2.StartTime);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(observable1.StartTime, Is.EqualTo(originalStartTime));
            Assert.That(observable2.StartTime, Is.EqualTo(originalStartTime));
            Assert.That(observable3.StartTime, Is.EqualTo(originalStartTime));
        }
    }

    [Test]
    public void Constructor_WithNullStartTime_DefaultsToCurrentTime()
    {
        // Arrange
        var args = CreateMockArgs(lockedUntil: DateTimeOffset.UtcNow.AddSeconds(30));
        DateTimeOffset? nullStartTime = null;

        // Act
        var observable = new MessageLockObservable(args, nullStartTime);

        // Assert
        Assert.That(observable.StartTime, Is.Not.Null);
        Assert.That(observable.StartTime!.Value, Is.EqualTo(DateTimeOffset.UtcNow).Within(TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void StartTime_AllowsCalculatingElapsedTime_ForMaxDurationCheck()
    {
        // Arrange
        // Simulate a message that started processing 3 minutes ago
        var startTime = DateTimeOffset.UtcNow.AddMinutes(-3);
        var args = CreateMockArgs(lockedUntil: DateTimeOffset.UtcNow.AddSeconds(30));

        // Act
        var observable = new MessageLockObservable(args, startTime);
        var elapsedTime = DateTimeOffset.UtcNow - observable.StartTime!.Value;

        // Assert
        // Elapsed time should be approximately 3 minutes
        Assert.That(elapsedTime.TotalMinutes, Is.GreaterThanOrEqualTo(3));
        Assert.That(elapsedTime.TotalMinutes, Is.LessThan(3.1));

        // This is the key check for max duration enforcement
        var maxDuration = TimeSpan.FromMinutes(5);
        var shouldContinueRenewing = elapsedTime < maxDuration;
        Assert.That(shouldContinueRenewing, Is.True);
    }

    private static ProcessMessageEventArgs CreateMockArgs(DateTimeOffset lockedUntil)
    {
        var lockToken = Guid.NewGuid();
        var lockTokenBytes = BinaryData.FromString(lockToken.ToString());

        var bytes = new List<ReadOnlyMemory<byte>> { new byte[] { 1, 2, 3 } };
        var messageBody = new AmqpMessageBody(bytes);
        var amqpMessage = new AmqpAnnotatedMessage(messageBody);

        amqpMessage.MessageAnnotations.Add("x-opt-locked-until", lockedUntil.DateTime);
        var msg = ServiceBusReceivedMessage.FromAmqpMessage(amqpMessage, lockTokenBytes);

        return new ProcessMessageEventArgs(
            msg,
            It.IsAny<ServiceBusReceiver>(),
            It.IsAny<CancellationToken>());
    }
}