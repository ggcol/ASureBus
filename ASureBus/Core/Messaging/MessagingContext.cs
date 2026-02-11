using ASureBus.Abstractions;
using ASureBus.Abstractions.Options.Messaging;
using ASureBus.IO.Heavies;

namespace ASureBus.Core.Messaging;

internal sealed class MessagingContext(IMessageEmitter emitter, IHeavyIO heavyIO)
    : CollectMessage(heavyIO), IMessagingContext
{
    public IMessagingContext Bind(Guid correlationId)
    {
        CorrelationId = correlationId;
        return this;
    }
    
    public Task Send<TCommand>(TCommand message, CancellationToken cancellationToken = default)
        where TCommand : IAmACommand
    {
        return EnqueueAndFlush(message, null, cancellationToken);
    }

    public Task Send<TCommand>(TCommand message, SendOptions options,
        CancellationToken cancellationToken = default)
        where TCommand : IAmACommand
    {
        return options.IsScheduled
            ? EnqueueAndFlush(message, options.ScheduledTime!.Value, options, cancellationToken)
            : EnqueueAndFlush(message, options, cancellationToken);
    }

    public Task SendAfter<TCommand>(TCommand message, TimeSpan delay,
        CancellationToken cancellationToken = default)
        where TCommand : IAmACommand
    {
        return SendScheduled(message, DateTimeOffset.UtcNow.Add(delay), cancellationToken);
    }

    public Task SendScheduled<TCommand>(TCommand message, DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
        where TCommand : IAmACommand
    {
        return EnqueueAndFlush(message, scheduledTime, null, cancellationToken);
    }

    public Task Publish<TEvent>(TEvent message, CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent
    {
        return EnqueueAndFlush(message, null, cancellationToken);
    }

    public Task Publish<TEvent>(TEvent message, PublishOptions options,
        CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent
    {
        return options.IsScheduled
            ? EnqueueAndFlush(message, options.ScheduledTime!.Value, options, cancellationToken)
            : EnqueueAndFlush(message, options, cancellationToken);
    }

    public Task PublishAfter<TEvent>(TEvent message, TimeSpan delay,
        CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent
    {
        return PublishScheduled(message, DateTimeOffset.UtcNow.Add(delay), cancellationToken);
    }

    public Task PublishScheduled<TEvent>(TEvent message, DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent
    {
        return EnqueueAndFlush(message, scheduledTime, null, cancellationToken);
    }

    private async Task EnqueueAndFlush<TMessage>(TMessage message, EmitOptions? options,
        CancellationToken cancellationToken)
        where TMessage : IAmAMessage
    {
        await Enqueue(message, options, cancellationToken).ConfigureAwait(false);
        await emitter.FlushAll(this, cancellationToken).ConfigureAwait(false);
    }

    private async Task EnqueueAndFlush<TMessage>(TMessage message, DateTimeOffset scheduledTime,
        EmitOptions? options, CancellationToken cancellationToken)
        where TMessage : IAmAMessage
    {
        await Enqueue(message, scheduledTime, options, cancellationToken).ConfigureAwait(false);
        await emitter.FlushAll(this, cancellationToken).ConfigureAwait(false);
    }
}