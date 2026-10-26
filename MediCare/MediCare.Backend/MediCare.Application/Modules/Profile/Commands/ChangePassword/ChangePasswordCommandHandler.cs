namespace MediCare.Application.Modules.Profile.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler(
    IAppDbContext db,
    IAppCurrentUser currentUser,
    IPasswordHasher<Users> hasher)
    : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new MediCareNotFoundException("User not found.");

        var check = hasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (check == PasswordVerificationResult.Failed)
            throw new MediCareConflictException("Trenutna lozinka nije ispravna.");

        user.PasswordHash = hasher.HashPassword(user, request.NewPassword);
        user.TokenVersion++; // marks all previously issued tokens as old

        // Sign out everywhere: refresh tokens can no longer be used to get new access tokens
        var now = DateTime.UtcNow;
        var activeTokens = await db.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(ct);

        foreach (var rt in activeTokens)
        {
            rt.IsRevoked = true;
            rt.RevokedAtUtc = now;
        }

        await db.SaveChangesAsync(ct);
    }
}
