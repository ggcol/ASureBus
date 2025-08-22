using ASureBus.Abstractions.Configurations;

namespace ASureBus.ConfigurationObjects.Options;

public sealed class HeavyPropertiesOptions : IConfigureHeavyProperties
{
    public string? ConnectionString { get; set; }
    public string? Container { get; set; }
}