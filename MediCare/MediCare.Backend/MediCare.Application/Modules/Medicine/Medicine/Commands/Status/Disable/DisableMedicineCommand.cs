namespace MediCare.Application.Modules.Medicine.Medicine.Commands.Status.Disable;

public sealed class DisableMedicineCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
