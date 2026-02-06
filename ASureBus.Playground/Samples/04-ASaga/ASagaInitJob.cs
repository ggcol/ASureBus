using ASureBus.Abstractions;
using ASureBus.Playground.Samples._04_ASaga.Messages;
using Microsoft.Extensions.Hosting;

namespace ASureBus.Playground.Samples._04_ASaga;

internal class ASagaInitJob(
    IMessagingContext context,
    IHostApplicationLifetime hostApplicationLifetime)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await context.Send(new ASagaInitCommand(), cancellationToken)
            .ConfigureAwait(false);
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        hostApplicationLifetime.StopApplication();
        return Task.CompletedTask;
    }
}