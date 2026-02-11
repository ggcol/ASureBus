using ASureBus.Abstractions.Configurations;

namespace ASureBus.ConfigurationObjects.Config;

public sealed class FileSystemSagaPersistenceConfig : IConfigureFileSystemSagaPersistence
{
    private string? _rootDirectoryPath;
    public string? RootDirectoryPath 
    { 
        get => _rootDirectoryPath ?? Defaults.FileSystemSagaPersistence.ROOT_DIRECTORY_PATH; 
        set => _rootDirectoryPath = value; 
    }
}