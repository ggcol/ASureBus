using ASureBus.Abstractions;
using ASureBus.Playground.Samples._01_OneCommand.Messages;
using Microsoft.Extensions.Hosting;

namespace ASureBus.Playground.Samples._01_OneCommand;

internal class OneCommandInitJob(
    IMessagingContext context,
    IHostApplicationLifetime hostApplicationLifetime)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var max = new Random().Next(1, 10);

        for (var i = 0; i <= max; i++)
        {
            await context.Send(new ACommand
                {
                    Something = $"{i} - Hello world!"
                }, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        hostApplicationLifetime.StopApplication();
        return Task.CompletedTask;
    }
}