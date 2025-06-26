using ASureBus.Abstractions;
using ASureBus.Accessories.Heavy;

namespace ASureBus.Core.Entities;

internal class AsbMessage<TMessage> : IAsbMessage
    where TMessage : IAmAMessage
{
    public required AsbMessageHeader Header { get; set; }
    public required TMessage Message { get; init; }
}

internal class AsbMessageHeader
{
    public required Guid MessageId { get; set; }
    public required Guid CorrelationId { get; init; }
    public required string MessageName { get; init; }
    public required string Destination { get; init; }
    public required bool IsCommand { get; init; }
    public IReadOnlyList<HeavyReference>? Heavies { get; set; }
    public bool IsScheduled => ScheduledTime.HasValue;
    public DateTimeOffset? ScheduledTime { get; set; }
}