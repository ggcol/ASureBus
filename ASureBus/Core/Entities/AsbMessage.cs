using ASureBus.Abstractions;

namespace ASureBus.Core.Entities;

internal class AsbMessage<TMessage> : IAsbMessage
    where TMessage : IAmAMessage
{
    public required AsbMessageHeader Header { get; set; }
    public required TMessage Message { get; init; }
}