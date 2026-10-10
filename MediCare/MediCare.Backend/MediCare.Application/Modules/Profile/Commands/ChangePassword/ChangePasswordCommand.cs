namespace MediCare.Application.Modules.Profile.Commands.ChangePassword;

/// <summary>
/// Changes the logged-in user's password.
/// All refresh tokens are revoked, so every device has to log in again with the new password.
/// </summary>
public sealed class ChangePasswordCommand : IRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
