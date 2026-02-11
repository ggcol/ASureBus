using System.Text.Json.Serialization;

namespace ASureBus.IO.StorageAccount;

internal interface IAzureDataStorageService
{
    internal Task Save<TItem>(TItem item, string containerName,
        string blobName, bool overwrite = default,
        CancellationToken cancellationToken = default);

    internal Task<object?> Get(string containerName, string blobName,
        Type returnType, JsonConverter? converter = null,
        CancellationToken cancellationToken = default);

    internal Task Delete(string containerName, string blobName,
        CancellationToken cancellationToken = default);
}