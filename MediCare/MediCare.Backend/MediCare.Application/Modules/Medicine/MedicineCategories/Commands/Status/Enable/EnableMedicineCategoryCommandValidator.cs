namespace MediCare.Application.Modules.Medicine.MedicineCategories.Commands.Status.Enable;

public sealed class EnableMedicineCategoryCommandValidator : AbstractValidator<EnableMedicineCategoryCommand>
{
    public EnableMedicineCategoryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
