﻿using Microsoft.Extensions.Hosting;
using Rsb;
using Rsb.Services;

namespace Playground.Samples.OneMessage;

internal class OneMessageInitJob : IHostedService
{
    private readonly IMessagingContext _context;
    private readonly IMessageEmitter _emitter;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public OneMessageInitJob(IMessagingContext context,
        IMessageEmitter emitter,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _context = context;
        _emitter = emitter;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _context.Send(new AMessage()
            {
                Something = "Hello world!"
            }, cancellationToken)
            .ConfigureAwait(false);

        await _emitter.FlushAll((ICollectMessage)_context, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _hostApplicationLifetime.StopApplication();
    }
}