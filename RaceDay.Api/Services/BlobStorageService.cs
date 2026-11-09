using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services;

public class BlobStorageService : IImageStorage
{
    private const long MaxBytes = 5 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    private readonly IConfiguration _configuration;
    private BlobContainerClient? _container;

    public BlobStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> UploadAsync(IFormFile file, string folder)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("No file was supplied.");

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException("Only JPG, PNG, GIF and WEBP images are allowed.");
        if (file.Length > MaxBytes)
            throw new ArgumentException("The image must be 5 MB or smaller.");

        var container = GetContainer();
        await EnsureContainerAsync(container);

        string blobName = $"{folder}/{Guid.NewGuid()}{extension}";
        BlobClient blob = container.GetBlobClient(blobName);

        await using Stream stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

        return blob.Uri.ToString();
    }

    private BlobContainerClient GetContainer()
    {
        if (_container is not null) return _container;

        string? connectionString = _configuration["AzureBlobStorage:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString) || connectionString == "SET_WITH_USER_SECRETS")
            throw new InvalidOperationException(
                "Azure Blob Storage is not configured. Set AzureBlobStorage:ConnectionString with user secrets.");

        string containerName = _configuration["AzureBlobStorage:ContainerName"] ?? "raceday-images";
        _container = new BlobContainerClient(connectionString, containerName);
        return _container;
    }

    private static async Task EnsureContainerAsync(BlobContainerClient container)
    {
        try
        {
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob);
        }
        catch (RequestFailedException)
        {
            // The storage account may not allow public blob access. Create a private container instead.
            await container.CreateIfNotExistsAsync();
        }
    }
}
