using ASureBus.Abstractions.Configurations;

namespace ASureBus.ConfigurationObjects;

public sealed class DataStorageSagaPersistenceConfig : IConfigureDataStorageSagaPersistence
{
    public required string ConnectionString { get; set; }
    public required string Container { get; set; }
}

public sealed class DataStorageSagaPersistenceOptions : IConfigureDataStorageSagaPersistence
{
    public string? ConnectionString { get; set; }
    public string? Container { get; set; }
}