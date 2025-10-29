namespace MediCare.Application.Modules.Catalog.ProductCategories.Queries.List;

public sealed class ListMedicineCategoriesQueryDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required bool IsEnabled { get; init; }
}
