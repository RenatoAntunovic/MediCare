using MediCare.Application.Modules.Catalog.Medicine.Commands.Create;

public sealed class CreateMedicineCommandValidator
    : AbstractValidator<CreateMedicineCommand>
{
    public CreateMedicineCommandValidator()
    {
        RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(Medicine.Constraints.NameMaxLength)
                .WithMessage($"Name can be at most {Medicine.Constraints.NameMaxLength} characters long.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(Medicine.Constraints.DescriptionMaxLength)
            .WithMessage($"Description can be at most {Medicine.Constraints.DescriptionMaxLength} characters long.");

        RuleFor(x => x.MedicineCategoryId)
            .GreaterThan(0).WithMessage("Medicine category must be selected.");

        RuleFor(x => x.ImagePath)
            .NotEmpty().WithMessage("Image path is required.");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0.");
    }
}