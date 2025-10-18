using APCleaningBackend.Services;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

public class AzureBlobUploader : IBlobUploader
{
    private readonly IConfiguration _config;

    public AzureBlobUploader(IConfiguration config)
    {
        _config = config;
    }

    public async Task<string> UploadAsync(IFormFile file, string containerName)
    {
        var connectionString = _config["Azure:StorageConnectionString"];
        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var blobClient = containerClient.GetBlobClient(fileName);

        using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, overwrite: true);

        return fileName;
    }

    public async Task DeleteAsync(string fileName, string containerName)
    {
        var connectionString = _config["Azure:StorageConnectionString"];
        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.DeleteIfExistsAsync();
    }
}