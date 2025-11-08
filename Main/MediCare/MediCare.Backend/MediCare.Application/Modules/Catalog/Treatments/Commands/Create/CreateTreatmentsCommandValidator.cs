using MediCare.Application.Modules.Catalog.Treatments.Commands.Create;

public sealed class CreateTreatmentsCommandValidator
    : AbstractValidator<CreateTreatmentsCommand>
{
    public CreateTreatmentsCommandValidator()
    {
        RuleFor(x => x.ServiceName)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(Medicine.Constraints.NameMaxLength)
                .WithMessage($"Name can be at most {Medicine.Constraints.NameMaxLength} characters long.");
    }
}