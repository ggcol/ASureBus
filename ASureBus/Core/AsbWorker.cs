using ASureBus.Core.MessageProcessing;
using ASureBus.Core.MessageProcessing.LockHandling;
using ASureBus.Core.TypesHandling;
using ASureBus.Core.TypesHandling.Entities;
using ASureBus.Services.ServiceBus;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;

namespace ASureBus.Core;

internal sealed class AsbWorker : IHostedService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    private readonly Dictionary<ListenerType, ServiceBusProcessor> _processors = new();

    public AsbWorker(
        IHostApplicationLifetime hostApplicationLifetime,
        IAzureServiceBusService azureServiceBusService,
        ITypesLoader typesLoader,
        IProcessSagaMessages sagaProcessor,
        IProcessHandlerMessages handlerProcessor,
        IMessageLockObserver messageLockObserver)
    {
        _hostApplicationLifetime = hostApplicationLifetime;

        foreach (var handler in typesLoader.Handlers)
        {
            var processor = azureServiceBusService
                .GetProcessor(handler, hostApplicationLifetime.ApplicationStopping)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            processor.ProcessMessageAsync += async args =>
            {
                if (AsbConfiguration.EnableMessageLockAutoRenewal)
                {
                    messageLockObserver.RenewOnExpiration(args);
                }

                await handlerProcessor.ProcessMessage(handler, args).ConfigureAwait(false);
            };

            processor.ProcessErrorAsync += async args
                => await handlerProcessor.ProcessError(handler, args).ConfigureAwait(false);

            _processors.Add(handler, processor);
        }

        foreach (var saga in typesLoader.Sagas)
        {
            foreach (var listener in saga.Listeners)
            {
                var processor = azureServiceBusService
                    .GetProcessor(listener, hostApplicationLifetime.ApplicationStopping)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                processor.ProcessMessageAsync += async args =>
                {
                    if (AsbConfiguration.EnableMessageLockAutoRenewal)
                    {
                        messageLockObserver.RenewOnExpiration(args);
                    }

                    await sagaProcessor.ProcessMessage(saga, listener, args).ConfigureAwait(false);
                };

                processor.ProcessErrorAsync += async args
                    => await sagaProcessor.ProcessError(saga, listener, args).ConfigureAwait(false);

                _processors.Add(listener, processor);
            }
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var processor in _processors.Values)
        {
            await processor
                .StartProcessingAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var processor in _processors.Values)
        {
            await processor
                .StopProcessingAsync(cancellationToken)
                .ConfigureAwait(false);

            await processor
                .DisposeAsync()
                .ConfigureAwait(false);
        }

        _hostApplicationLifetime.StopApplication();
    }
}