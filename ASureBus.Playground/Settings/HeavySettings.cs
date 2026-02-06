using ASureBus.Abstractions.Configurations;

namespace ASureBus.Playground.Settings;

public class HeavySettings : IConfigureHeavyProperties
{
    public string ConnectionString { get; set; } = null!;
    public string Container { get; set; } = null!;
}