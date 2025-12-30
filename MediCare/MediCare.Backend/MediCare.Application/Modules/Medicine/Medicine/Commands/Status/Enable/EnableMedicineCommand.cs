namespace MediCare.Application.Modules.Medicine.Medicine.Commands.Status.Enable;

public sealed class EnableMedicineCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
