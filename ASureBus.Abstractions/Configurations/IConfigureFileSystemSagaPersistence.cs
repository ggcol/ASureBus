namespace ASureBus.Abstractions.Configurations;

public interface IConfigureFileSystemSagaPersistence
{
    public string? RootDirectoryPath { get; set; }
}