namespace MediCare.Application.Modules.Medicine.Medicine.Commands.Status.Disable;

/// <summary>Rejects a non-positive id before the handler runs (same rule as the other id-only commands).</summary>
public sealed class DisableMedicineCommandValidator : AbstractValidator<DisableMedicineCommand>
{
    public DisableMedicineCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
