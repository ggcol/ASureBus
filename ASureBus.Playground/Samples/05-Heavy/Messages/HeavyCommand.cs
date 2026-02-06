using ASureBus.Abstractions;

namespace ASureBus.Playground.Samples._05_Heavy.Messages;

public class HeavyCommand : IAmACommand
{
    public Heavy<string>? AHeavyProp { get; init; }
}