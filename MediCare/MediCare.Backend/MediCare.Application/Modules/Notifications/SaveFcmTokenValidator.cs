namespace MediCare.Application.Modules.Notifications;

public sealed class SaveFcmTokenCommandValidator : AbstractValidator<SaveFcmTokenCommand>
{
    public SaveFcmTokenCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.")
            .MaximumLength(500);
    }
}