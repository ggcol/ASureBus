using ASureBus.ConfigurationObjects.Options;
using ASureBus.Core;
using ASureBus.Core.MessageProcessing.LockHandling.Entities;
using Azure.Core.Amqp;
using Azure.Messaging.ServiceBus;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.MessageProcessing.LockHandling.Entities;

[TestFixture]
public class MessageLockObservableSimpleTests
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
    public void Constructor_WithFutureLockedUntil_CreatesObservable()
    {
        // Arrange
        var args = CreateMockArgs(DateTimeOffset.UtcNow.AddSeconds(60));
        
        // Act
        var observable = new MessageLockObservable(args);
        
        // Assert
        Assert.That(observable.HasExpiration, Is.True);
    }

    [Test]
    public void Constructor_WithExpiredLockedUntil_StillCreatesObservableWithDefaultDuration()
    {
        // Arrange
        var args = CreateMockArgs(DateTimeOffset.UtcNow.AddSeconds(-10));
        
        // Act
        var observable = new MessageLockObservable(args);
        
        // Act
        Assert.That(observable.HasExpiration, Is.True);
    }

    [Test]
    public void Constructor_WithDuration_CreatesObservable()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(30);
        var startTime = DateTimeOffset.UtcNow;
        
        // Act
        var observable = new MessageLockObservable(duration, startTime);
           
        // Assert
        using (Assert.EnterMultipleScope())
        {

            Assert.That(observable.HasExpiration, Is.True);
            Assert.That(observable.StartTime, Is.EqualTo(startTime));
        }
    }

    private ProcessMessageEventArgs CreateMockArgs(DateTimeOffset lockedUntil)
    {
        var lockTokenBytes = BinaryData.FromString(Guid.NewGuid().ToString());
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