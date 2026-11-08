using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace RaceDay.Api.Services;

public class BlobStorageService : IBlobStorageService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly Dictionary<string, string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
        ["image/gif"] = ".gif"
    };

    private readonly AzureBlobOptions _options;
    private readonly ILogger<BlobStorageService> _logger;
    private BlobContainerClient? _container;

    public BlobStorageService(IOptions<AzureBlobOptions> options, ILogger<BlobStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("Please choose an image file to upload.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new ArgumentException("The image is too large. The maximum size is 5 MB.");
        }

        if (!AllowedTypes.TryGetValue(file.ContentType ?? string.Empty, out var extension))
        {
            throw new ArgumentException("Only JPG, PNG, WEBP or GIF images are allowed.");
        }

        var container = await GetContainerAsync(cancellationToken);

        // A GUID blob name avoids collisions and never trusts the client-supplied file name.
        var blobName = $"{folder.Trim('/')}/{Guid.NewGuid():N}{extension}";
        var blob = container.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(
            stream,
            new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType } },
            cancellationToken);

        _logger.LogInformation("Uploaded image to blob {BlobName}", blobName);
        return blob.Uri.ToString();
    }

    private async Task<BlobContainerClient> GetContainerAsync(CancellationToken cancellationToken)
    {
        if (_container is not null)
        {
            return _container;
        }

        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            throw new InvalidOperationException(
                "Azure Blob Storage is not configured. Set AzureBlob:ConnectionString (user-secrets or environment variable).");
        }

        var client = new BlobContainerClient(_options.ConnectionString, _options.ContainerName);

        // Public read access at blob level so the MVC pages can display the images by URL.
        await client.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        _container = client;
        return _container;
    }
}
