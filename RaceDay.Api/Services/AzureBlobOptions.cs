namespace RaceDay.Api.Services;

/// <summary>Bound from the "AzureBlob" configuration section.</summary>
public class AzureBlobOptions
{
    public const string SectionName = "AzureBlob";

    /// <summary>Storage account connection string (user-secrets / environment variable — never committed).</summary>
    public string? ConnectionString { get; set; }

    /// <summary>Container that holds event banners and profile pictures.</summary>
    public string ContainerName { get; set; } = "raceday-images";
}
