namespace MediCare.Application.Modules.Catalog.Medicine.Commands.Delete;

public class DeleteMedicineCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
