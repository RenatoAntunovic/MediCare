using MediCare.Application.Modules.Profile.Queries.GetMyProfile;

namespace MediCare.Application.Modules.Profile.Commands.UpdateMyProfile;

/// <summary>
/// Updates the logged-in user's personal data.
/// Email and username are NOT editable (email is used to log in).
/// </summary>
public sealed class UpdateMyProfileCommand : IRequest<MyProfileDto>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}
