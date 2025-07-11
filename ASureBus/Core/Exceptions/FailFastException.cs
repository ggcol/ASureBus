using ASureBus.Abstractions;

namespace ASureBus.Core.Exceptions;

internal sealed class FailFastException : Exception, IFailFast
{
    public FailFastException(string message) : base(message)
    {
    }
}