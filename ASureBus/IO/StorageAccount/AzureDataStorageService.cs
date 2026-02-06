using System.Text;
using System.Text.Json.Serialization;
using ASureBus.Utils;
using Azure.Storage.Blobs;

namespace ASureBus.IO.StorageAccount;

internal sealed class AzureDataStorageService(string? connectionString)
    : IAzureDataStorageService
{
    public async Task Save<TItem>(TItem item, string containerName,
        string blobName, bool overwrite = default,
        CancellationToken cancellationToken = default)
    {
        var containerClient =
            await MakeContainerClient(containerName, cancellationToken)
                .ConfigureAwait(false);
        var blobClient = containerClient.GetBlobClient(blobName);

        var jsonString = Serializer.Serialize(item);
        var itemBytes = Encoding.UTF8.GetBytes(jsonString);

        using var uploadStream = new MemoryStream(itemBytes);
        await blobClient.UploadAsync(uploadStream, overwrite, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<object?> Get(string containerName, string blobName,
        Type returnType, JsonConverter? converter = null,
        CancellationToken cancellationToken = default)
    {
        var containerClient =
            await MakeContainerClient(containerName, cancellationToken)
                .ConfigureAwait(false);
        var blobClient = containerClient.GetBlobClient(blobName);

        var downloadInfo = await blobClient
            .OpenReadAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        using var reader = new StreamReader(downloadInfo);
        var read = await reader.ReadToEndAsync(cancellationToken)
            .ConfigureAwait(false);

        return converter is null
            ? Serializer.Deserialize(read, returnType)
            : Serializer.Deserialize(read, returnType, converter);
    }

    public async Task Delete(string containerName, string blobName,
        CancellationToken cancellationToken = default)
    {
        var containerClent =
            await MakeContainerClient(containerName, cancellationToken)
                .ConfigureAwait(false);
        var blobClient = containerClent.GetBlobClient(blobName);

        _ = await blobClient
            .DeleteAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        //return?
    }

    private async Task<BlobContainerClient> MakeContainerClient(
        string containerName, CancellationToken cancellationToken)
    {
        var containerClient =
            MakeServiceClient().GetBlobContainerClient(containerName);

        if (!await containerClient.ExistsAsync(cancellationToken)
                .ConfigureAwait(false))
        {
            await containerClient
                .CreateAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        return containerClient;
    }

    private BlobServiceClient MakeServiceClient()
    {
        return new BlobServiceClient(connectionString);
    }
}