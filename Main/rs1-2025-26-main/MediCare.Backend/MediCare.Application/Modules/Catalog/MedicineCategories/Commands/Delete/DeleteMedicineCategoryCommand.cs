namespace MediCare.Application.Modules.Catalog.ProductCategories.Commands.Delete;

public class DeleteMedicineCategoryCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
