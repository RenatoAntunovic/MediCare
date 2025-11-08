namespace MediCare.Application.Modules.Catalog.MedicineCategories.Commands.Delete;

public class DeleteMedicineCategoryCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
