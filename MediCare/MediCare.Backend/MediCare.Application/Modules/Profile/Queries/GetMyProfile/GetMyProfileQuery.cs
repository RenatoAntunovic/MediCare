namespace MediCare.Application.Modules.Profile.Queries.GetMyProfile;

/// <summary>Returns the logged-in user's profile (the user id comes from the JWT).</summary>
public sealed class GetMyProfileQuery : IRequest<MyProfileDto>;
