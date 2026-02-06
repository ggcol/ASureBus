using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ASureBus.Playground;

public class AGenericJob(
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<AGenericJob> logger)
    : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        //do sth
        return Task.CompletedTask; 
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Bye!");
        hostApplicationLifetime.StopApplication();
        return Task.CompletedTask;
    }
}