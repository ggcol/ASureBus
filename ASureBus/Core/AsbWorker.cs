﻿using ASureBus.Accessories.Heavy;
using ASureBus.Core.Caching;
using ASureBus.Core.Enablers;
using ASureBus.Core.Entities;
using ASureBus.Core.Exceptions;
using ASureBus.Core.Messaging;
using ASureBus.Core.Sagas;
using ASureBus.Core.TypesHandling;
using ASureBus.Core.TypesHandling.Entities;
using ASureBus.Services.ServiceBus;
using ASureBus.Utils;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ASureBus.Core;

internal sealed class AsbWorker : IHostedService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageEmitter _messageEmitter;
    private readonly IAsbCache _cache;
    private readonly ISagaBehaviour _sagaBehaviour;

    private readonly ISagaIO? _sagaIo = AsbConfiguration.OffloadSagas
        ? new SagaIO()
        : null;

    private readonly IDictionary<ListenerType, ServiceBusProcessor>
        _processors = new Dictionary<ListenerType, ServiceBusProcessor>();

    public AsbWorker(
        IHostApplicationLifetime hostApplicationLifetime,
        IServiceProvider serviceProvider,
        IAzureServiceBusService azureServiceBusService,
        IMessageEmitter messageEmitter,
        ITypesLoader typesLoader,
        IAsbCache cache,
        ISagaBehaviour sagaBehaviour)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _serviceProvider = serviceProvider;
        _messageEmitter = messageEmitter;
        _cache = cache;
        _sagaBehaviour = sagaBehaviour;

        foreach (var handler in typesLoader.Handlers)
        {
            var processor = azureServiceBusService
                .GetProcessor(handler, hostApplicationLifetime.ApplicationStopping)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            processor.ProcessMessageAsync += async args
                => await ProcessMessage(handler, args);

            processor.ProcessErrorAsync += async args
                => await ProcessError(handler, args);

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
                    await ProcessMessage(saga, listener, args);

                processor.ProcessErrorAsync += async args
                    => await ProcessError(saga, listener, args);

                _processors.Add(listener, processor);
            }
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var processorKvp in _processors)
        {
            await processorKvp.Value
                .StartProcessingAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var processorKvp in _processors)
        {
            await processorKvp.Value
                .StopProcessingAsync(cancellationToken)
                .ConfigureAwait(false);

            await processorKvp.Value
                .DisposeAsync()
                .ConfigureAwait(false);
        }

        _hostApplicationLifetime.StopApplication();
    }

    private async Task ProcessError(HandlerType handlerType,
        ProcessErrorEventArgs args)
    {
        var broker = BrokerFactory.Get(_serviceProvider, handlerType);

        await broker.HandleError(args.Exception, args.CancellationToken)
            .ConfigureAwait(false);
    }

    private async Task ProcessMessage(HandlerType handlerType,
        ProcessMessageEventArgs args)
    {
        var correlationId = await GetCorrelationId(args);

        var broker = BrokerFactory.Get(_serviceProvider, handlerType, correlationId);

        var asbMessage = await broker
            .Handle(args.Message.Body, args.CancellationToken)
            .ConfigureAwait(false);

        if (UsesHeavies(asbMessage))
        {
            await DeleteHeavies(asbMessage, args.CancellationToken).ConfigureAwait(false);
        }

        await args
            .CompleteMessageAsync(args.Message, args.CancellationToken)
            .ConfigureAwait(false);

        await _messageEmitter.FlushAll(broker.Collector, args.CancellationToken)
            .ConfigureAwait(false);
    }

    private async Task ProcessError(SagaType sagaType, SagaHandlerType listenerType,
        ProcessErrorEventArgs args)
    {
        var ex = args.Exception as AsbException;
        var correlationId = ex!.CorrelationId;

        var implSaga = await GetConcreteSaga(sagaType, listenerType, correlationId)
                .ConfigureAwait(false);

        var broker = BrokerFactory.Get(_serviceProvider, sagaType, implSaga, listenerType,
            correlationId);

        await broker.HandleError(ex?.OriginalException, args.CancellationToken)
            .ConfigureAwait(false);
    }

    private async Task ProcessMessage(SagaType sagaType,
        SagaHandlerType listenerType, ProcessMessageEventArgs args)
    {
        var correlationId = await GetCorrelationId(args);

        try
        {
            var implSaga = await GetConcreteSaga(sagaType, listenerType, correlationId)
                    .ConfigureAwait(false);

            var broker = BrokerFactory.Get(_serviceProvider, sagaType, implSaga, listenerType,
                correlationId);

            var asbMessage = await broker.Handle(args.Message.Body, args.CancellationToken)
                .ConfigureAwait(false);

            if (UsesHeavies(asbMessage))
            {
                await DeleteHeavies(asbMessage, args.CancellationToken).ConfigureAwait(false);
            }

            await args.CompleteMessageAsync(args.Message, args.CancellationToken)
                .ConfigureAwait(false);

            if (_sagaIo is not null)
            {
                await _sagaIo.Unload(implSaga, correlationId, sagaType).ConfigureAwait(false);
            }

            _cache.Upsert(correlationId, implSaga);

            await _messageEmitter.FlushAll(broker.Collector, args.CancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            /*
             * exception caught here is always TargetInvocationException
             * since every saga's handle method is called by reflection
             * and the actual exception should be stored in InnerException
             */ 
            
            if (ex.InnerException is AsbException) throw;
            throw new AsbException
            {
                OriginalException = ex.InnerException ?? ex,
                CorrelationId = correlationId
            };
        }
    }

    private static async Task<Guid> GetCorrelationId(ProcessMessageEventArgs args)
    {
        var corrId = await Serializer.Deserialize<DeserializeCorrelationId>(
                args.Message.Body.ToStream(),
                cancellationToken: args.CancellationToken)
            .ConfigureAwait(false);

        return corrId.CorrelationId;
    }

    private async Task<object?> GetConcreteSaga(SagaType sagaType, SagaHandlerType listenerType,
        Guid correlationId)
    {
        if (_cache.TryGetValue(correlationId, out var saga))
        {
            return saga;
        }

        if (_sagaIo is not null)
        {
            saga = await _sagaIo.Load(correlationId, sagaType)
                .ConfigureAwait(false);

            if (saga is not null)
            {
                return _cache.Set(correlationId, saga);
            }
        }

        if (!listenerType.IsInitMessageHandler)
        {
            throw new SagaNotFoundException(sagaType.Type, correlationId);
        }

        saga = ActivatorUtilities.CreateInstance(_serviceProvider, sagaType.Type);

        _sagaBehaviour.SetCorrelationId(sagaType, correlationId, saga);

        _sagaBehaviour.HandleCompletion(sagaType, correlationId, saga);

        return _cache.Set(correlationId, saga);
    }
    
    private static bool UsesHeavies(IAsbMessage asbMessage)
    {
        return HeavyIo.IsHeavyConfigured()
               && asbMessage.Heavies is not null
               && asbMessage.Heavies.Any();
    }

    private static async Task DeleteHeavies(IAsbMessage asbMessage,
        CancellationToken cancellationToken)
    {
        foreach (var heavyRef in asbMessage.Heavies!)
        {
            await HeavyIo.Delete(asbMessage.MessageId, heavyRef, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}