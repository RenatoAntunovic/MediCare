namespace MediCare.Application.Modules.Profile;

/// <summary>
/// Changes the logged-in user's password.
/// All refresh tokens are revoked, so every device has to log in again with the new password.
/// </summary>
public sealed class ChangePasswordCommand : IRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        // Same minimum as registration
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
            .MaximumLength(100)
            .NotEqual(x => x.CurrentPassword).WithMessage("The new password must be different from the current one.");
    }
}

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