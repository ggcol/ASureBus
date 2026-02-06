using ASureBus.Abstractions;
using ASureBus.Playground.Samples._07_DelayedAndScheduled.Messages;
using Microsoft.Extensions.Hosting;

namespace ASureBus.Playground.Samples._07_DelayedAndScheduled;

public class DelayedAndScheduledInitJob(
    IMessagingContext context,
    IHostApplicationLifetime hostApplicationLifetime) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var delay = TimeSpan.FromSeconds(20);

        var message = new DelayedMessage
        {
            CreatedAt = DateTimeOffset.UtcNow,
            Delay = delay
        };

        await context.SendAfter(message, delay, cancellationToken)
            .ConfigureAwait(false);
        
        // alternative way to send a delayed message
        // await context.Send(message, new SendOptions
        //     {
        //         Delay = delay
        //     }, cancellationToken)
        //     .ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        hostApplicationLifetime.StopApplication();
        return Task.CompletedTask;
    }
}