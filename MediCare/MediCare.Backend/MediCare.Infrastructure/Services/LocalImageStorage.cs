using MediCare.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace MediCare.Infrastructure.Services;

/// <summary>Saves images into wwwroot/images (same location the handlers used before).</summary>
public sealed class LocalImageStorage : IImageStorage
{
    private const string WebRoot = "wwwroot";
    private const string ImagesFolder = "images";

    public async Task<string> SaveAsync(IFormFile file, CancellationToken ct)
    {
        var uploadsFolder = Path.Combine(WebRoot, ImagesFolder);
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        try
        {
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream, ct);
        }
        catch
        {
            // a half-written file must not stay on disk
            if (File.Exists(filePath))
                File.Delete(filePath);
            throw;
        }

        return $"{ImagesFolder}/{uniqueFileName}";
    }

    public void Delete(string relativePath)
    {
        var fullPath = Path.Combine(WebRoot, relativePath.TrimStart('/', '\\'));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}
