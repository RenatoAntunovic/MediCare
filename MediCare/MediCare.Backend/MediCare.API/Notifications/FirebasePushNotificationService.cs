using FirebaseAdmin.Messaging;
using MediCare.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MediCare.API.Notifications;

/// <summary>
/// Sends notifications through Firebase Cloud Messaging to the token saved on the user.
/// </summary>
public sealed class FirebasePushNotificationService(
    IAppDbContext db,
    ILogger<FirebasePushNotificationService> logger) : IPushNotificationService
{
    public async Task SendToUserAsync(int userId, string title, string body, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (string.IsNullOrWhiteSpace(user?.FcmToken))
            return; // user never allowed notifications

        try
        {
            var message = new Message
            {
                Token = user.FcmToken,
                Notification = new Notification { Title = title, Body = body }
            };

            await FirebaseMessaging.DefaultInstance.SendAsync(message, ct);
        }
        catch (FirebaseMessagingException ex) when (
            ex.MessagingErrorCode is MessagingErrorCode.Unregistered or MessagingErrorCode.InvalidArgument)
        {
            // The token is no longer valid (browser data cleared, permission revoked) → forget it
            logger.LogInformation("FCM token for user {UserId} is no longer valid, removing it.", userId);
            user.FcmToken = null;
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Never break the request because of a notification
            logger.LogWarning(ex, "Push notification to user {UserId} failed.", userId);
        }
    }
}