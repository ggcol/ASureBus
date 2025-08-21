using ASureBus.Abstractions.Configurations;

namespace ASureBus.ConfigurationObjects;

public sealed class HeavyPropertiesConfig : IConfigureHeavyProperties
{
    public required string ConnectionString { get; set; }
    public required string Container { get; set; }
}

public sealed class HeavyPropertiesOptions : IConfigureHeavyProperties
{
    public string? ConnectionString { get; set; }
    public string? Container { get; set; }
}