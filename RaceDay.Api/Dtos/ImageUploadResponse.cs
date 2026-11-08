namespace RaceDay.Api.Dtos;

/// <summary>Returned by the banner / profile-picture upload endpoints.</summary>
public class ImageUploadResponse
{
    public string Url { get; set; } = string.Empty;
}
