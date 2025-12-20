namespace MediCare.Application.Modules.Medicine.MedicineCategories.Commands.Delete;

public class DeleteMedicineCategoryCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
