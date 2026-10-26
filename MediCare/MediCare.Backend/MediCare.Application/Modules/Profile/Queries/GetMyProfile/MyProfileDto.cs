namespace MediCare.Application.Modules.Profile.Queries.GetMyProfile;

/// <summary>Profile data of the logged-in user (shown and edited on the Settings page).</summary>
public sealed class MyProfileDto
{
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }

    /// <summary>Single place where a user entity is mapped to the profile DTO (used by the query and the update command).</summary>
    public static MyProfileDto From(Users user) => new()
    {
        Email = user.Email,
        UserName = user.UserName,
        FirstName = user.FirstName,
        LastName = user.LastName,
        PhoneNumber = user.PhoneNumber,
        Address = user.Adress,
        City = user.City,
        DateOfBirth = user.DateOfBirth
    };
}
