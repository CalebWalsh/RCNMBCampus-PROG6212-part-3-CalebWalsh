namespace RaceDay.Mvc.Services;

public static class ImageValidator
{
    private const long MaxBytes = 5 * 1024 * 1024;
    private static readonly string[] Allowed = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public static string? Validate(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Allowed.Contains(extension))
            return "Choose a JPG, PNG, GIF or WEBP image.";
        if (file.Length > MaxBytes)
            return "The image must be 5 MB or smaller.";
        return null;
    }
}
