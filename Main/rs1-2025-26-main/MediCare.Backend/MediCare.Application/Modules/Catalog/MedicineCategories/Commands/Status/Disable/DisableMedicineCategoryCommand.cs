namespace MediCare.Application.Modules.Catalog.ProductCategories.Commands.Status.Disable;

public sealed class DisableMedicineCategoryCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
