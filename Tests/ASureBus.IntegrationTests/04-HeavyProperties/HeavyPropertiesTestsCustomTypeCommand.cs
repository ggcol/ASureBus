using ASureBus.Abstractions;

namespace ASureBus.IntegrationTests._04_HeavyProperties;

internal sealed record HeavyPropertiesTestsCustomTypeCommand(Heavy<ACustomType> OffloadedProperty, ACustomType OnBoardProperty)
    : IAmACommand;