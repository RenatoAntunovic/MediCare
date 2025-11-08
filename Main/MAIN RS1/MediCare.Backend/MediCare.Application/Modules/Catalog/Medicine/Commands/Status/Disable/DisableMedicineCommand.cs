namespace MediCare.Application.Modules.Catalog.Medicine.Commands.Status.Disable;

public sealed class DisableMedicineCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
