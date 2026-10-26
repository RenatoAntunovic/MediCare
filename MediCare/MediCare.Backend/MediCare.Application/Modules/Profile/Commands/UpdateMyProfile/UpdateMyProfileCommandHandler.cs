using MediCare.Application.Modules.Profile.Queries.GetMyProfile;

namespace MediCare.Application.Modules.Profile.Commands.UpdateMyProfile;

public sealed class UpdateMyProfileCommandHandler(IAppDbContext db, IAppCurrentUser currentUser)
    : IRequestHandler<UpdateMyProfileCommand, MyProfileDto>
{
    public async Task<MyProfileDto> Handle(UpdateMyProfileCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new MediCareNotFoundException("User not found.");

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = request.PhoneNumber.Trim();
        user.Adress = request.Address.Trim();
        user.City = request.City.Trim();
        user.DateOfBirth = request.DateOfBirth.Date;

        await db.SaveChangesAsync(ct);

        // Return the saved data so the form shows exactly what is now in the database
        return MyProfileDto.From(user);
    }
}
