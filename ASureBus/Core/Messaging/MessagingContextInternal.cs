using ASureBus.Abstractions;
using ASureBus.Abstractions.Options.Messaging;
using ASureBus.IO.Heavies;

namespace ASureBus.Core.Messaging;

internal sealed class MessagingContextInternal(IHeavyIO heavyIO) : CollectMessage(heavyIO), IMessagingContext
{
    public IMessagingContext Bind(Guid correlationId)
    {
        CorrelationId = correlationId;
        return this;
    }
    
    public async Task Send<TCommand>(TCommand message,
        CancellationToken cancellationToken = default)
        where TCommand : IAmACommand
    {
        await Enqueue(message, null, cancellationToken).ConfigureAwait(false);
    }

    public Task Send<TCommand>(TCommand message, SendOptions options,
        CancellationToken cancellationToken = default)
        where TCommand : IAmACommand
    {
        return options.IsScheduled
            ? Enqueue(message, options.ScheduledTime!.Value, options, cancellationToken)
            : Enqueue(message, options, cancellationToken);
    }

    public async Task SendAfter<TCommand>(TCommand message, TimeSpan delay,
        CancellationToken cancellationToken = default) where TCommand : IAmACommand
    {
        await SendScheduled(message, DateTimeOffset.UtcNow.Add(delay), cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task SendScheduled<TCommand>(TCommand message, DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default) where TCommand : IAmACommand
    {
        await Enqueue(message, scheduledTime, null, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Publish<TEvent>(TEvent message, CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent
    {
        await Enqueue(message, null, cancellationToken)
            .ConfigureAwait(false);
    }

    public Task Publish<TEvent>(TEvent message, PublishOptions options,
        CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent
    {
        return options.IsScheduled
            ? Enqueue(message, options.ScheduledTime!.Value, options, cancellationToken)
            : Enqueue(message, options, cancellationToken);
    }

    public async Task PublishAfter<TEvent>(TEvent message, TimeSpan delay,
        CancellationToken cancellationToken = default) where TEvent : IAmAnEvent
    {
        await PublishScheduled(message, DateTimeOffset.UtcNow.Add(delay), cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task PublishScheduled<TEvent>(TEvent message, DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default) where TEvent : IAmAnEvent
    {
        await Enqueue(message, scheduledTime, null, cancellationToken)
            .ConfigureAwait(false);
    }
}