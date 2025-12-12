using ASureBus.Abstractions;
using ASureBus.Core.Caching;
using ASureBus.Core.Entities.NotNullPatternReturns;
using ASureBus.Core.Exceptions;
using ASureBus.Core.TypesHandling.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ASureBus.Core.Sagas;

internal interface ISagaFactory
{
    internal Task<object?> Get(SagaType sagaType, SagaHandlerType listenerType,
        Guid correlationId, CancellationToken cancellationToken);
}

internal sealed class SagaFactory(
    IAsbCache cache,
    ISagaIO sagaIo,
    IServiceProvider serviceProvider)
    : ISagaFactory
{
    public async Task<object?> Get(SagaType sagaType, SagaHandlerType listenerType,
        Guid correlationId, CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(correlationId, out var saga))
        {
            return saga;
        }

        saga = await sagaIo.Load(correlationId, sagaType, cancellationToken)
            .ConfigureAwait(false);

        if (saga is not null)
        {
            await HandleCompletion((ISaga)saga, sagaType, cancellationToken).ConfigureAwait(false);
            return cache.Set(correlationId, saga);
        }

        // if is timeout ok, may be the saga already completed
        if (listenerType.IsTimeoutHandler)
        {
            return new SagaAlreadyCompleted(sagaType, correlationId);
        }

        if (!listenerType.IsInitMessageHandler)
        {
            throw new SagaNotFoundException(sagaType.Type, correlationId);
        }

        saga = ActivatorUtilities.CreateInstance(serviceProvider, sagaType.Type);

        if ((saga as ISaga)!.CorrelationId != Guid.Empty)
        {
            correlationId = (saga as ISaga)!.CorrelationId;
        }
        else
        {
            SetCorrelationId(sagaType, correlationId, saga);
        }

        await HandleCompletion((ISaga)saga, sagaType, cancellationToken).ConfigureAwait(false);
        return cache.Set(correlationId, saga);
    }

    private static void SetCorrelationId(SagaType sagaType, Guid correlationId, object sagaInstance)
    {
        sagaType.Type
            .GetProperty(nameof(ISaga.CorrelationId))?
            .SetValue(sagaInstance, correlationId);
    }

    private Task HandleCompletion(ISaga saga, SagaType sagaType, CancellationToken cancellationToken)
    {
        saga.Completed += async (_, args) =>
        {
            cache.Remove(args.CorrelationId);
            await sagaIo.Delete(args.CorrelationId, sagaType, cancellationToken).ConfigureAwait(false);
        };
        return Task.CompletedTask;
    }
}