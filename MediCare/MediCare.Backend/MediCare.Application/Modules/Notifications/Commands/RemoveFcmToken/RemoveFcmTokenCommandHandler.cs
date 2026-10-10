namespace MediCare.Application.Modules.Notifications.Commands.RemoveFcmToken;

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
