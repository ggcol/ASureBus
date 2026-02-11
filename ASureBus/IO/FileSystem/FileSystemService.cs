using System.Text.Json.Serialization;
using ASureBus.Utils;

namespace ASureBus.IO.FileSystem;

internal sealed class FileSystemService : IFileSystemService
{
    private readonly string _root = AsbConfiguration.FileSystemSagaPersistence!.RootDirectoryPath!;

    public FileSystemService()
    {
        if (!Directory.Exists(_root))
        {
            Directory.CreateDirectory(_root);
        }
    }

    public async Task Save<TItem>(TItem item, string directory, string fileName,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(Path.Combine(_root, directory)))
        {
            Directory.CreateDirectory(Path.Combine(_root, directory));
        }

        var path = Path.Combine(_root, directory, fileName) + Ext.Json;
        var serialized = Serializer.Serialize(item);
        await File.WriteAllTextAsync(path, serialized, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<object?> Get(string directory, string fileName, Type returnType,
        JsonConverter? converter = null,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(Path.Combine(_root, directory)))
        {
            return null;
        }

        var path = Path.Combine(_root, directory, fileName) + Ext.Json;
        if (!File.Exists(path)) return null;

        var raw = await File.ReadAllTextAsync(path, cancellationToken)
            .ConfigureAwait(false);

        return converter is null
            ? Serializer.Deserialize(raw, returnType)
            : Serializer.Deserialize(raw, returnType, converter);
    }

    public void Delete(string directory, string fileName)
    {
        if (!Directory.Exists(Path.Combine(_root, directory)))
        {
            return;
        }

        var path = Path.Combine(_root, directory, fileName) + Ext.Json;
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}