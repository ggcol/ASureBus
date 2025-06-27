namespace ASureBus.Abstractions;

public interface IHandleTimeout<in TTimeout> : IHandleMessage<TTimeout>
    where TTimeout : IAmATimeout
{
    
}