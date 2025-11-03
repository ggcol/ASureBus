using ASureBus.Abstractions.Behaviours;

namespace ASureBus.Abstractions;

public abstract class Heavy : ObservableExpirable
{
    internal Guid Ref = Guid.NewGuid();
    
    protected Heavy(TimeSpan? expiresAfter) : base(expiresAfter)
    { }
    
    protected Heavy(): this(null)
    { }
}

public class Heavy<T> : Heavy
{
    public T? Value { get; set; }

    public Heavy()
    { }
    
    public Heavy(T value) : this()
    {
        Value = value;
    }
    
    public Heavy(T value, TimeSpan expiresAfter) : base(expiresAfter)
    {
        Value = value;
    }

}