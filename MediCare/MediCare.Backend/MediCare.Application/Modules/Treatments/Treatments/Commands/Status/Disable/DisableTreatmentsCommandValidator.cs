namespace MediCare.Application.Modules.Catalog.Treatments.Commands.Status.Disable;

/// <summary>Rejects a non-positive id before the handler runs (same rule as the other id-only commands).</summary>
public sealed class DisableTreatmentsCommandValidator : AbstractValidator<DisableTreatmentsCommand>
{
    public DisableTreatmentsCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
