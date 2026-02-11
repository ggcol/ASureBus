using ASureBus.Accessories.Heavies.Entities;

namespace ASureBus.Core.Entities;

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