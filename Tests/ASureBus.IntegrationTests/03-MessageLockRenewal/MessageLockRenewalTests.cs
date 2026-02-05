using ASureBus.Abstractions;
using ASureBus.IntegrationTests.TestConfigurations;

namespace ASureBus.IntegrationTests._03_MessageLockRenewal;

public class MessageLockRenewalTests : WithAsbHostAndCheckService
{
    [OneTimeSetUp]
    public void Setup()
    {
        RunHost();
    }

    [Test]
    public async Task LongRunningHandler_WithLockRenewalEnabled_CompletesSuccessfully()
    {
        // Arrange
        CheckService.Reset();

        const int handlerProcessingTimeInSeconds = 3;
        
        // Act
        await Context.Send(new MessageLockRenewalLongRunningCommand
        {
            ProcessingTimeInSeconds = handlerProcessingTimeInSeconds,
            MessageId = Guid.NewGuid().ToString()
        }).ConfigureAwait(false);

        const int waitTimeInSeconds = 5;
        await Task.Delay(TimeSpan.FromSeconds(waitTimeInSeconds));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(CheckService.Acknowledged, Is.True);
            Assert.That(CheckService.ProcessingTimeSeconds, Is.GreaterThanOrEqualTo(handlerProcessingTimeInSeconds));
        });
    }

    [Test]
    public async Task VeryLongRunningHandler_ExceedingMaxDuration_MayTimeout()
    {
        // Arrange
        CheckService.Reset();

        const int handlerProcessingTimeInSeconds = 10;
        
        // Act
        await Context.Send(new MessageLockRenewalLongRunningCommand
        {
            ProcessingTimeInSeconds = handlerProcessingTimeInSeconds,
            MessageId = Guid.NewGuid().ToString()
        }).ConfigureAwait(false);

        const int waitTimeInSeconds = 12;
        await Task.Delay(TimeSpan.FromSeconds(waitTimeInSeconds));

        // Assert
        const int notProcessedYet = 0;
        const double timingToleranceInSeconds = 0.5;
        Assert.That(CheckService.ProcessingTimeSeconds,
            Is.GreaterThanOrEqualTo(handlerProcessingTimeInSeconds - timingToleranceInSeconds)
                .Or.EqualTo(notProcessedYet));
    }

    [Test]
    public async Task MultipleMessages_WithLockRenewal_AllCompleteSuccessfully()
    {
        // Arrange
        CheckService.Reset();

        const int messageCount = 5;
        
        // Act
        for (var i = 0; i < messageCount; i++)
        {
            await Context.Send(new MessageLockRenewalLongRunningCommand
            {
                ProcessingTimeInSeconds = 2,
                MessageId = $"msg-{i}"
            }).ConfigureAwait(false);
        }

        const int processingTimePerMessageInSeconds = 2;
        const int bufferInSeconds = 3;
        const int totalWaitTimeInSeconds = messageCount * processingTimePerMessageInSeconds + bufferInSeconds;
        await Task.Delay(TimeSpan.FromSeconds(totalWaitTimeInSeconds));

        // Assert
        Assert.That(CheckService.ProcessedMessageCount, Is.EqualTo(messageCount));
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await StopHost().ConfigureAwait(false);
    }
}