﻿using Asb.Abstractions;
using Microsoft.Extensions.Hosting;

namespace Playground.Samples._05_Heavy;

public class HeavyInitJob(
    IMessagingContext context,
    IHostApplicationLifetime hostApplicationLifetime) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await context.Send(new HeavyCommand
            {
                AHeavyProp = new("Hello world!")
            }, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        hostApplicationLifetime.StopApplication();
    }
}