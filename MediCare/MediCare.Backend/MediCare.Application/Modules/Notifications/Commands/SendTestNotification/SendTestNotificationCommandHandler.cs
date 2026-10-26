namespace MediCare.Application.Modules.Notifications.Commands.SendTestNotification;

public sealed class SendTestNotificationCommandHandler(IPushNotificationService push, IAppCurrentUser currentUser)
    : IRequestHandler<SendTestNotificationCommand>
{
    public async Task Handle(SendTestNotificationCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();

        await push.SendToUserAsync(userId, "MediCare test", "Notifikacije rade!", ct);
    }
}
