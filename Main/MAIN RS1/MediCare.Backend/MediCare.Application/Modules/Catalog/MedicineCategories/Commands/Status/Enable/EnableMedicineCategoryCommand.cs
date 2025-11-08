namespace MediCare.Application.Modules.Catalog.MedicineCategories.Commands.Status.Enable;

public sealed class EnableMedicineCategoryCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
