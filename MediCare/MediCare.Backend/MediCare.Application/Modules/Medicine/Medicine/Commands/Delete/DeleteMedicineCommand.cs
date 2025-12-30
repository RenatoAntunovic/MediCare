namespace MediCare.Application.Modules.Medicine.Medicine.Commands.Delete;

public class DeleteMedicineCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
