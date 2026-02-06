using ASureBus.Abstractions;
using ASureBus.Playground.Samples._05_Heavy.Messages;
using Microsoft.Extensions.Hosting;

namespace ASureBus.Playground.Samples._05_Heavy;

public class HeavyInitJob(
    IMessagingContext context,
    IHostApplicationLifetime hostApplicationLifetime)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await context.Send(new HeavyCommand
            {
                AHeavyProp = new Heavy<string>("Hello world!")
            }, cancellationToken)
            .ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        hostApplicationLifetime.StopApplication();
        return Task.CompletedTask;
    }
}