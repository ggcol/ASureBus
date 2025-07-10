namespace ASureBus.Core.Exceptions;

internal sealed class FailFastException : Exception
{
    public FailFastException()
    {
        
    }

    public FailFastException(string message) : base(message)
    {
    }
}