namespace MediCare.Application.Modules.Profile.Queries.GetMyProfile;

public sealed class GetMyProfileQueryHandler(IAppDbContext db, IAppCurrentUser currentUser)
    : IRequestHandler<GetMyProfileQuery, MyProfileDto>
{
    public async Task<MyProfileDto> Handle(GetMyProfileQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();

        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new MediCareNotFoundException("User not found.");

        return MyProfileDto.From(user);
    }
}
