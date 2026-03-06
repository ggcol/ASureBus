using ASureBus.IntegrationTests.TestConfigurations;

namespace ASureBus.IntegrationTests._02_PublishingAnEventToConsumer;

public class PublishingAnEventToConsumerTests : WithAsbHostAndCheckService
{
    
    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        await RunHost().ConfigureAwait(false);
    }
    
    [Test]
    public async Task PublishingAnEventToConsumer()
    {
        //Act
        await Context.Publish(new PublishingAnEventToConsumerEvent()).ConfigureAwait(false);
        Thread.Sleep(500);

        //Assert
        Assert.That(CheckService.Acknowledged, Is.True);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await StopHost().ConfigureAwait(false);
    }
}