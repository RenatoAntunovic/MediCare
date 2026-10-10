namespace MediCare.Application.Modules.Profile;

public sealed class GetMyProfileQuery : IRequest<MyProfileDto>;

public sealed class GetMyProfileQueryHandler(IAppDbContext db, IAppCurrentUser currentUser)
    : IRequestHandler<GetMyProfileQuery, MyProfileDto>
{
    public async Task<MyProfileDto> Handle(GetMyProfileQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();

        return await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new MyProfileDto
            {
                Email = u.Email,
                UserName = u.UserName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                PhoneNumber = u.PhoneNumber,
                Address = u.Adress,
                City = u.City,
                DateOfBirth = u.DateOfBirth
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new MediCareNotFoundException("User not found.");
    }
}