using ASureBus.Abstractions.Behaviours;

namespace ASureBus.Abstractions;

public abstract class Heavy(TimeSpan? expiresAfter) 
    : ObservableExpirable(expiresAfter)
{
    internal Guid Ref = Guid.NewGuid();
}

public class Heavy<T>(T value, TimeSpan? expiresAfter = null) 
    : Heavy(expiresAfter)
{
    public T? Value { get; } = value;
}