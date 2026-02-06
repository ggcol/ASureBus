// Copyright (c) 2025 Gianluca Colombo (red.Co)
//
// This file is part of ASureBus (https://github.com/ggcol/ASureBus).
//
// ASureBus is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation, either version 3 of
// the License, or (at your option) any later version.
//
// ASureBus is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with ASureBus. If not, see <https://www.gnu.org/licenses/>.

using ASureBus.Core.MessageProcessing;
using ASureBus.Core.MessageProcessing.LockHandling;
using ASureBus.Core.TypesHandling;
using ASureBus.Core.TypesHandling.Entities;
using ASureBus.Services.ServiceBus;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;

namespace ASureBus;

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
            try
            {
                await processor
                    .StopProcessingAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (ObjectDisposedException)
            {
                // Processor already disposed during shutdown
            }

            try
            {
                await processor
                    .DisposeAsync()
                    .ConfigureAwait(false);
            }
            catch (ObjectDisposedException)
            {
                // Processor already disposed
            }
        }

        _hostApplicationLifetime.StopApplication();
    }
}