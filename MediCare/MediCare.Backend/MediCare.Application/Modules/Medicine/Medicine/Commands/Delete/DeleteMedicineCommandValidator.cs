namespace MediCare.Application.Modules.Medicine.Medicine.Commands.Delete;

/// <summary>Rejects a non-positive id before the handler runs (same rule as the other id-only commands).</summary>
public sealed class DeleteMedicineCommandValidator : AbstractValidator<DeleteMedicineCommand>
{
    public DeleteMedicineCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
