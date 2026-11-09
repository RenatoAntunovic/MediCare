namespace MediCare.Application.Modules.Medicine.MedicineCategories.Commands.Delete;

/// <summary>Rejects a non-positive id before the handler runs (same rule as the other id-only commands).</summary>
public sealed class DeleteMedicineCategoryCommandValidator : AbstractValidator<DeleteMedicineCategoryCommand>
{
    public DeleteMedicineCategoryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
