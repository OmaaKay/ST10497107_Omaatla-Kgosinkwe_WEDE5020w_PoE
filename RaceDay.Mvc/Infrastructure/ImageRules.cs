namespace RaceDay.Mvc.Infrastructure;

/// <summary>Quick, friendly pre-check of an image before it is sent to the API (the API validates again).</summary>
public static class ImageRules
{
    private static readonly string[] AllowedTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };
    public const long MaxBytes = 5 * 1024 * 1024;

    public static string? Validate(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null; // optional upload

        if (file.Length > MaxBytes) return "The image is too large. The maximum size is 5 MB.";
        if (!AllowedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            return "Only JPG, PNG, WEBP or GIF images are allowed.";
        return null;
    }
}
