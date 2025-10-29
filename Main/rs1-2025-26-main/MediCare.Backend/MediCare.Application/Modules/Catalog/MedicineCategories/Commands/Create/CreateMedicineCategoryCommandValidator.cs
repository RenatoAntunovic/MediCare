namespace MediCare.Application.Modules.Catalog.ProductCategories.Commands.Create;

public sealed class CreateMedicineCategoryCommandValidator
    : AbstractValidator<CreateMedicineCategoryCommand>
{
    public CreateMedicineCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(MedicineCategories.Constraints.NameMaxLength).WithMessage($"Name can be at most {MedicineCategories.Constraints.NameMaxLength} characters long.");
    }
}