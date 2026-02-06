using ASureBus.Abstractions.Configurations;

namespace ASureBus.Playground.Settings;

public class FIleSystemSagaPersistenceSettings : IConfigureFileSystemSagaPersistence
{
    public string? RootDirectoryPath { get; set; }
}