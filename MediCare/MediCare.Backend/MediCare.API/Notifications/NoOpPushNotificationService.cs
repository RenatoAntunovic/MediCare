using MediCare.Application.Abstractions;

namespace MediCare.API.Notifications;

/// <summary>
/// Used when firebase-adminsdk.json is missing: the app works normally, notifications are just skipped.
/// </summary>
public sealed class NoOpPushNotificationService : IPushNotificationService
{
    public Task SendToUserAsync(int userId, string title, string body, CancellationToken ct = default)
        => Task.CompletedTask;
}