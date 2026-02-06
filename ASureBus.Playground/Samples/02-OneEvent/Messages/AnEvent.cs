using ASureBus.Abstractions;

namespace ASureBus.Playground.Samples._02_OneEvent.Messages;

public class AnEvent : IAmAnEvent
{
    public string? Something { get; init; }
}