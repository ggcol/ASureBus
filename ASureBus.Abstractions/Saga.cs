namespace ASureBus.Abstractions;

internal interface ISaga
{
    public Guid CorrelationId { get; set; }
    internal event EventHandler<SagaCompletedEventArgs>? Completed;
    internal bool IsComplete { get; }
}

public abstract class Saga<T> : ISaga
    where T : SagaData, new()
{
    public T SagaData { get; internal set; } = new();
    public Guid CorrelationId { get; set; } = Guid.Empty;
    public event EventHandler<SagaCompletedEventArgs>? Completed;
    public bool IsComplete { get; private set; }

    protected void IAmComplete()
    {
        IsComplete = true;
        Completed?.Invoke(this, new SagaCompletedEventArgs
        {
            CorrelationId = CorrelationId,
            Type = GetType()
        });
    }

    protected async Task RequestTimeout<TTimeout>(TTimeout timeout, TimeSpan delay, IMessagingContext context,
        CancellationToken cancellationToken = default)
        where TTimeout : IAmATimeout
    {
        await context.SendAfter(timeout, delay, cancellationToken).ConfigureAwait(false);
    }

    protected async Task RequestTimeout<TTimeout>(TimeSpan delay, IMessagingContext context,
        CancellationToken cancellationToken = default)
        where TTimeout : IAmATimeout, new()
        => await RequestTimeout(new TTimeout(), delay, context, cancellationToken);

    protected async Task RequestTimeout<TTimeout>(TTimeout timeout, DateTimeOffset scheduledTime,
        IMessagingContext context, CancellationToken cancellationToken = default)
        where TTimeout : IAmATimeout
    {
        await context.SendScheduled(timeout, scheduledTime, cancellationToken).ConfigureAwait(false);
    }

    protected async Task RequestTimeout<TTimeout>(DateTimeOffset scheduledTime, IMessagingContext context,
        CancellationToken cancellationToken = default)
        where TTimeout : IAmATimeout, new()
        => await RequestTimeout(new TTimeout(), scheduledTime, context, cancellationToken);
}

public sealed class SagaCompletedEventArgs : EventArgs
{
    internal Guid CorrelationId { get; init; }
    internal Type? Type { get; init; }
}