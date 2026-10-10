namespace MediCare.Application.Modules.Notifications.Commands.SaveFcmToken;

public sealed class SaveFcmTokenCommandHandler(IAppDbContext db, IAppCurrentUser currentUser)
    : IRequestHandler<SaveFcmTokenCommand>
{
    public async Task Handle(SaveFcmTokenCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();

        // One browser = one token. If another user logged in on this browser before,
        // take the token away from them so they don't get this user's notifications.
        var previousOwners = await db.Users
            .Where(u => u.FcmToken == request.Token && u.Id != userId)
            .ToListAsync(ct);
        foreach (var u in previousOwners)
            u.FcmToken = null;

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new MediCareNotFoundException("User not found.");

        user.FcmToken = request.Token;
        await db.SaveChangesAsync(ct);
    }
}
