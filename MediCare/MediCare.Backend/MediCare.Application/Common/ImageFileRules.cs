using Microsoft.AspNetCore.Http;

namespace MediCare.Application.Common;

/// <summary>
/// Shared FluentValidation rule for uploaded images (medicines, treatments).
/// A null file passes, so the rule also works for optional images on update.
/// </summary>
public static class ImageFileRules
{
    public const long MaxSizeBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".jfif"];
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/pjpeg", "image/png", "image/webp"];

    public static IRuleBuilderOptions<T, TFile> MustBeValidImage<T, TFile>(this IRuleBuilder<T, TFile> rule)
        where TFile : IFormFile?
    {
        return rule
            .Must(f => f is null || f.Length <= MaxSizeBytes)
                .WithMessage("Image can be at most 5 MB.")
            .Must(f => f is null || AllowedExtensions.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()))
                .WithMessage("Allowed image formats: .jpg, .jpeg, .png, .webp, .jfif.")
            .Must(f => f is null || AllowedContentTypes.Contains(f.ContentType.ToLowerInvariant()))
                .WithMessage("The uploaded file is not a supported image.");
    }
}