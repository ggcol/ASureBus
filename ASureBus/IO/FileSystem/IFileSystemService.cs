using System.Text.Json.Serialization;

namespace ASureBus.IO.FileSystem;

internal interface IFileSystemService
{
    internal Task Save<TItem>(TItem item, string directory, string fileName,
        CancellationToken cancellationToken = default);

    internal Task<object?> Get(string directory, string fileName, Type returnType, JsonConverter? converter = null,
        CancellationToken cancellationToken = default);

    internal void Delete(string directory, string fileName);
}