namespace RaceDay.Api.Services;

public interface IImageStorage
{
    /// <summary>Uploads an image and returns its public URL. Throws ArgumentException for an invalid file.</summary>
    Task<string> UploadAsync(IFormFile file, string folder);
}
