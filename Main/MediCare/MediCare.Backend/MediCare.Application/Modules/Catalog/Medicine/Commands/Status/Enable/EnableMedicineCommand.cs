namespace MediCare.Application.Modules.Catalog.Medicine.Commands.Status.Enable;

public sealed class EnableMedicineCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
