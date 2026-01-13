using ASureBus.Abstractions;

namespace ASureBus.IntegrationTests._03_MessageLockRenewal;

public record MessageLockRenewalLongRunningCommand : IAmACommand
{
    public required string MessageId { get; init; }
    public required int ProcessingTimeInSeconds { get; init; }
}