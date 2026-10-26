namespace MediCare.Application.Modules.Notifications.Commands.SaveFcmToken;

/// <summary>Registers this browser's FCM token for the logged-in user.</summary>
public sealed class SaveFcmTokenCommand : IRequest
{
    public string Token { get; set; } = string.Empty;
}
