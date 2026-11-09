namespace MediCare.Application.Modules.Catalog.Treatments.Commands.Status.Enable;

/// <summary>Rejects a non-positive id before the handler runs (same rule as the other id-only commands).</summary>
public sealed class EnableTreatmentsCommandValidator : AbstractValidator<EnableTreatmentsCommand>
{
    public EnableTreatmentsCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
