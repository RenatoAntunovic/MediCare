namespace MediCare.Application.Modules.Medicine.MedicineCategories.Commands.Status.Disable;

public sealed class DisableMedicineCategoryCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
