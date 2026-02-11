using ASureBus.Abstractions.Configurations;

namespace ASureBus.ConfigurationObjects.Options;

public sealed class FileSystemSagaPersistenceOptions : IConfigureFileSystemSagaPersistence
{
    public string? RootDirectoryPath { get; set; } = Defaults.FileSystemSagaPersistence.ROOT_DIRECTORY_PATH;
}