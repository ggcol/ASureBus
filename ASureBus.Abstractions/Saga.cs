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
    
    // ReSharper disable once InconsistentNaming
    protected void IAmComplete()
    {
        IsComplete = true;
        Completed?.Invoke(this, new SagaCompletedEventArgs
        {
            CorrelationId = CorrelationId,
            Type = GetType()
        });
    }
}

public sealed class SagaCompletedEventArgs : EventArgs
{
    internal Guid CorrelationId { get; init; }
    internal Type? Type { get; init; }
}
