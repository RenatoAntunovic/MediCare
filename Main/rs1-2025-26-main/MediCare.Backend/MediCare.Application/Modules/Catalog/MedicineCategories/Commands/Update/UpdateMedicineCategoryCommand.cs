namespace MediCare.Application.Modules.Catalog.ProductCategories.Commands.Update;

public sealed class UpdateMedicineCategoryCommand : IRequest<Unit>
{
    [JsonIgnore]
    public int Id { get; set; }
    public required string Name { get; set; }
}
