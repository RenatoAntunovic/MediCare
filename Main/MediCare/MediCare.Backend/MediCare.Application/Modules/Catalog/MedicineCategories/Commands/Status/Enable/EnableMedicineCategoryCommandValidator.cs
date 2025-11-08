namespace MediCare.Application.Modules.Catalog.MedicineCategories.Commands.Status.Enable;

public sealed class EnableMedicineCategoryCommandValidator : AbstractValidator<EnableMedicineCategoryCommand>
{
    public EnableMedicineCategoryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
