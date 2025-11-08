namespace MediCare.Application.Modules.Catalog.MedicineCategories.Commands.Status.Disable;

public sealed class DisableTreatmentCategoryCommandValidator : AbstractValidator<DisableMedicineCategoryCommand>
{
    public DisableTreatmentCategoryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
