namespace MediCare.Application.Modules.Notifications;

public sealed class SaveFcmTokenCommand : IRequest
{
    public string Token { get; set; } = string.Empty;
}