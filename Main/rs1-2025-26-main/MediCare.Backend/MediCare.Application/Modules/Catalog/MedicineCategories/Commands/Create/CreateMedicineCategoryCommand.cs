namespace MediCare.Application.Modules.Catalog.ProductCategories.Commands.Create;

public class CreateMedicineCategoryCommand : IRequest<int>
{
    public required string Name { get; set; }
}