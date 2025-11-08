using MediCare.Application.Modules.Catalog.MedicineCategories.Commands.Update;

public sealed class UpdateMedicineCategoryCommandValidator
    : AbstractValidator<UpdateMedicineCategoryCommand>
{
    public UpdateMedicineCategoryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(MedicineCategories.Constraints.NameMaxLength).WithMessage($"Name can be at most {MedicineCategories.Constraints.NameMaxLength} characters long.");
    }
}