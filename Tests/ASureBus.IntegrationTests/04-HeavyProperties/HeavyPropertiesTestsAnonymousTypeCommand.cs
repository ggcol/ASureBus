using ASureBus.Abstractions;

namespace ASureBus.IntegrationTests._04_HeavyProperties;

internal sealed record HeavyPropertiesTestsAnonymousTypeCommand(Heavy<object> OffloadedProperty, object OnBoardProperty)
    : IAmACommand;