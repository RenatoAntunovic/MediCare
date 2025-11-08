namespace MediCare.Application.Modules.Catalog.Medicine.Queries.List;

public sealed class ListMedicineQueryDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required decimal Price { get; init; }
    public required string Description { get; init; }
    public required int MedicineCategoryId { get; init; }
    public required string MedicineCategoryName { get; init; }
    public required string ImagePath { get; init; }
    public required int Weight { get; init; }
    public required bool isEnabled { get; init; }
}
