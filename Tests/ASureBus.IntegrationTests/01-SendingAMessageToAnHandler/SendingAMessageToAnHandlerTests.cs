using ASureBus.IntegrationTests.TestConfigurations;

namespace ASureBus.IntegrationTests._01_SendingAMessageToAnHandler;

public class SendingAMessageToAnHandlerTests : WithAsbHostAndCheckService
{
    [SetUp]
    public void Setup()
    {
        RunHost();
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
    
    [TearDown]
    public async Task TearDown()
    {
        await StopHost().ConfigureAwait(false);
    }
}