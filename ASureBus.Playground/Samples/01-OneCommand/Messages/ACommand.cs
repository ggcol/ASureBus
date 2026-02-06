using ASureBus.Abstractions;

namespace ASureBus.Playground.Samples._01_OneCommand.Messages;

public class ACommand : IAmACommand
{
    public string? Something { get; init; }
}