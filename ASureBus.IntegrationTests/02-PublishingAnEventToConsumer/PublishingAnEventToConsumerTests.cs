using ASureBus.IntegrationTests.TestConfigurations;

namespace ASureBus.IntegrationTests._02_PublishingAnEventToConsumer;

public class PublishingAnEventToConsumerTests : WithAsbHostAndCheckService
{
    
    [SetUp]
    public void Setup()
    {
        RunHost();
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

    [TearDown]
    public async Task TearDown()
    {
        await StopHost().ConfigureAwait(false);
    }
}