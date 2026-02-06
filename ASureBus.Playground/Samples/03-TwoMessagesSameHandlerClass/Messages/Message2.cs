using ASureBus.Abstractions;

namespace ASureBus.Playground.Samples._03_TwoMessagesSameHandlerClass.Messages;

public record Message2(string Something) : IAmACommand{}