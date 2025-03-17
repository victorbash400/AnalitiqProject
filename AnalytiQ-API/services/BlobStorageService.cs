using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic; // For Dictionary

public class BlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureBlobStorage:ConnectionString"];
        var containerName = configuration["AzureBlobStorage:ContainerName"];
        _containerClient = new BlobContainerClient(connectionString, containerName);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string tenantId, string batchId, string productName)
    {
        string blobName = $"{tenantId}/{batchId}/{fileName}";
        var blobClient = _containerClient.GetBlobClient(blobName);

        // Upload the file first
        await blobClient.UploadAsync(fileStream, true);

        // If productName is provided, set it as metadata
        if (!string.IsNullOrEmpty(productName))
        {
            var metadata = new Dictionary<string, string>
            {
                { "ProductName", productName }
            };
            await blobClient.SetMetadataAsync(metadata);
        }

        return blobClient.Uri.ToString(); // Return the uploaded file URL
    }
}