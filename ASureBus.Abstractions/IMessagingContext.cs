using ASureBus.Abstractions.Options.Messaging;

namespace ASureBus.Abstractions;

public interface IMessagingContext
{
    public Guid CorrelationId { get; }

    public Task Send<TCommand>(TCommand message, CancellationToken cancellationToken = default)
        where TCommand : IAmACommand;

    public IMessagingContext Bind(Guid correlationId);

    public Task Send<TCommand>(TCommand message, SendOptions options,
        CancellationToken cancellationToken = default)
        where TCommand : IAmACommand;

    public Task SendAfter<TCommand>(TCommand message, TimeSpan delay,
        CancellationToken cancellationToken = default)
        where TCommand : IAmACommand;

    public Task SendScheduled<TCommand>(TCommand message, DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
        where TCommand : IAmACommand;

    public Task Publish<TEvent>(TEvent message, CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent;

    public Task Publish<TEvent>(TEvent message, PublishOptions options,
        CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent;

    public Task PublishAfter<TEvent>(TEvent message, TimeSpan delay,
        CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent;

    public Task PublishScheduled<TEvent>(TEvent message, DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent;
}