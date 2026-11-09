namespace MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Delete;

/// <summary>Rejects a non-positive id before the handler runs (same rule as the other id-only commands).</summary>
public sealed class DeleteTreatmentCategoryCommandValidator : AbstractValidator<DeleteTreatmentCategoryCommand>
{
    public DeleteTreatmentCategoryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
