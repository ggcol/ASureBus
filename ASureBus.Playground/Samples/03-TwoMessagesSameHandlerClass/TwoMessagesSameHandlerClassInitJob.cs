using ASureBus.Abstractions;
using ASureBus.Playground.Samples._03_TwoMessagesSameHandlerClass.Messages;
using Microsoft.Extensions.Hosting;

namespace ASureBus.Playground.Samples._03_TwoMessagesSameHandlerClass;

internal class TwoMessagesSameHandlerClassInitJob(
    IMessagingContext context,
    IHostApplicationLifetime hostApplicationLifetime)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await context.Send(new Message1("Hello!"), cancellationToken)
            .ConfigureAwait(false);

        await context.Send(new Message2("World!"), cancellationToken)
            .ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        hostApplicationLifetime.StopApplication();
        return Task.CompletedTask;
    }
}