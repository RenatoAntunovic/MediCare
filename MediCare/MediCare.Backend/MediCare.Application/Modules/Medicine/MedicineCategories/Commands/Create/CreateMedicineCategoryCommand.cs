namespace MediCare.Application.Modules.Medicine.MedicineCategories.Commands.Create;

public class CreateMedicineCategoryCommand : IRequest<int>
{
    public required string Name { get; set; }
}