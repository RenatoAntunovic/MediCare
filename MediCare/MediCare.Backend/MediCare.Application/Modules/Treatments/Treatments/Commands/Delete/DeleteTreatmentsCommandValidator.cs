namespace MediCare.Application.Modules.Catalog.Treatments.Commands.Delete;

/// <summary>Rejects a non-positive id before the handler runs (same rule as the other id-only commands).</summary>
public sealed class DeleteTreatmentsCommandValidator : AbstractValidator<DeleteTreatmentsCommand>
{
    public DeleteTreatmentsCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
