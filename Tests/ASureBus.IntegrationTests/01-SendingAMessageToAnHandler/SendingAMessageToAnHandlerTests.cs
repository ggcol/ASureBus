using ASureBus.IntegrationTests.TestConfigurations;

namespace ASureBus.IntegrationTests._01_SendingAMessageToAnHandler;

public class SendingAMessageToAnHandlerTests : WithAsbHostAndCheckService
{
    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        await RunHost().ConfigureAwait(false);
    }
    
    [Test]
    public async Task SendingAMessageToAnHandler()
    {
        //Act
        await Context.Send(new SendingAMessageToHandlerCommand()).ConfigureAwait(false);
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