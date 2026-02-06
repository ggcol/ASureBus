using ASureBus.Abstractions;

namespace ASureBus.Playground.Samples._04_ASaga.Messages;

public class AnotherCommand : IAmACommand
{
    public string? Something { get; init; }   
}