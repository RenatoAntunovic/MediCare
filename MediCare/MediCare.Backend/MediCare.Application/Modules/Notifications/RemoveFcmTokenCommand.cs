namespace MediCare.Application.Modules.Notifications;

/// <summary>
/// Turns push notifications off for the logged-in user (removes the saved FCM token).
/// </summary>
public sealed class RemoveFcmTokenCommand : IRequest;

public sealed class RemoveFcmTokenCommandHandler(IAppDbContext db, IAppCurrentUser currentUser)
    : IRequestHandler<RemoveFcmTokenCommand>
{
    public async Task Handle(RemoveFcmTokenCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new MediCareNotFoundException("User not found.");

        user.FcmToken = null;
        await db.SaveChangesAsync(ct);
    }
}