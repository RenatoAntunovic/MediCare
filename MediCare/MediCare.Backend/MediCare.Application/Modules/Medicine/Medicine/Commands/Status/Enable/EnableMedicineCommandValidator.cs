namespace MediCare.Application.Modules.Medicine.Medicine.Commands.Status.Enable;

/// <summary>Rejects a non-positive id before the handler runs (same rule as the other id-only commands).</summary>
public sealed class EnableMedicineCommandValidator : AbstractValidator<EnableMedicineCommand>
{
    public EnableMedicineCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
