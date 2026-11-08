namespace RaceDay.Api.Services;

/// <summary>
/// Abstraction over Azure Blob Storage. Only the API talks to Azure — the MVC
/// application sends files to the API, which stores them through this service.
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Validates and uploads an image, returning its public URL.
    /// Throws <see cref="ArgumentException"/> for an invalid file and
    /// <see cref="InvalidOperationException"/> when Blob Storage is not configured.
    /// </summary>
    Task<string> UploadImageAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
}
