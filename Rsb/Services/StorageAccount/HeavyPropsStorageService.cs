﻿using Rsb.Core;
using Rsb.Utils;

namespace Rsb.Services.StorageAccount;

internal class HeavyPropsStorageService() 
    : AzureDataStorageService(RsbConfiguration.HeavyProps?.DataStorageConnectionString!)
{
    public async Task<object?> Get(string containerName, string blobName,
        Type returnType, CancellationToken cancellationToken = default)
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
    
        return Serializer.Deserialize(read, returnType);
    }
}