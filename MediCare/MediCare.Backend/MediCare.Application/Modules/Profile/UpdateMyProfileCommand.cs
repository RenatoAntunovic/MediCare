namespace MediCare.Application.Modules.Profile;

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

public sealed class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50);

        // Digits with optional +, spaces, "-" or "/" (e.g. 061-111-111 or +387 61 111 111)
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[\d\s\-/]{9,20}$").WithMessage("Invalid phone number format.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(100);

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(50);

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past.")
            .GreaterThan(new DateTime(1900, 1, 1)).WithMessage("Invalid date of birth.");
    }
}

public sealed class UpdateMyProfileCommandHandler(IAppDbContext db, IAppCurrentUser currentUser, ISender sender)
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

        // Return the fresh data so the form can show exactly what was saved
        return await sender.Send(new GetMyProfileQuery(), ct);
    }
}