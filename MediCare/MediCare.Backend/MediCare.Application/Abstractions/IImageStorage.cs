using Microsoft.AspNetCore.Http;

namespace MediCare.Application.Abstractions;

/// <summary>
/// Stores uploaded images on disk. Behind an interface so tests can use a fake storage
/// (for example one that fails on copy) without touching the real wwwroot folder.
/// </summary>
public interface IImageStorage
{
    /// <summary>Saves the file and returns its relative path, e.g. "images/{guid}_photo.png".</summary>
    Task<string> SaveAsync(IFormFile file, CancellationToken ct);

    /// <summary>Deletes a stored file. Does nothing if the file does not exist.</summary>
    void Delete(string relativePath);
}
