using ASureBus.Abstractions;
using ASureBus.Playground.Samples._06_SagaPersistence.Messages;
using Microsoft.Extensions.Hosting;

namespace ASureBus.Playground.Samples._06_SagaPersistence;

public class APersistedSagaInitJob(
    IHostApplicationLifetime applicationLifetime,
    IMessagingContext context)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await context.Send(new APersistedSagaInitCommand(), cancellationToken)
            .ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        applicationLifetime.StopApplication();
        return Task.CompletedTask;
    }
}