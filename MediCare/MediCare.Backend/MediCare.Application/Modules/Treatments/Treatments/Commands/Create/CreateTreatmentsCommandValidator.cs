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

        RuleFor(x => x.ImageFile)
           .NotNull().WithMessage("Image is required.")
           .MustBeValidImage();

        RuleFor(x => x.Price)
    .GreaterThan(0).WithMessage("Cijena mora biti veća od 0.");

        RuleFor(x => x.TreatmentCategoryId)
            .GreaterThan(0).WithMessage("Kategorija mora biti odabrana.");
    }
}