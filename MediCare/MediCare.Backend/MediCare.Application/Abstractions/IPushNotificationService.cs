namespace MediCare.Application.Abstractions;

/// <summary>
/// Sends push notifications to users (implemented with Firebase Cloud Messaging).
/// Implementations must never throw: a failed notification must not break the request.
/// </summary>
public interface IPushNotificationService
{
    Task SendToUserAsync(int userId, string title, string body, CancellationToken ct = default);
}