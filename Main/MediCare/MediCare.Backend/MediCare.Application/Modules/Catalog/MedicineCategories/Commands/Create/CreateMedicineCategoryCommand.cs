namespace MediCare.Application.Modules.Catalog.MedicineCategories.Commands.Create;

public class CreateMedicineCategoryCommand : IRequest<int>
{
    public required string Name { get; set; }
}