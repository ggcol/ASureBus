using ASureBus.Abstractions;

namespace ASureBus.IntegrationTests._04_HeavyProperties;

internal sealed record HeavyPropertiesTestsBasicTypeCommand(Heavy<string> OffloadedProperty, string OnBoardProperty)
    : IAmACommand;