using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using LaundryMgmt.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LaundryMgmt.Infrastructure.Services;

/// <summary>Uploads to Azure Blob Storage and returns the blob's own public URL directly
/// (no SAS token — catalog/promotion images are meant to be permanently, publicly
/// viewable, and a SAS would just add an expiry that eventually breaks them again the
/// same way local-disk storage did). Requires the target container to be created with
/// public "Blob" access level; see AddInfrastructure's config comment for setup.</summary>
public class AzureBlobImageStorageService : IImageStorageService
{
    private readonly BlobContainerClient _container;

    public AzureBlobImageStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "AzureStorage:ConnectionString is not configured — set it via `dotnet user-secrets` locally " +
                "or the AzureStorage__ConnectionString app setting in Azure.");

        var containerName = configuration["AzureStorage:ContainerName"];
        if (string.IsNullOrWhiteSpace(containerName))
            containerName = "uploads";

        _container = new BlobContainerClient(connectionString, containerName);
        _container.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient(fileName);
        await blob.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);
        return blob.Uri.ToString();
    }
}
