namespace MediCare.Application.Modules.Catalog.ProductCategories.Commands.Status.Enable;

public sealed class EnableMedicineCategoryCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
