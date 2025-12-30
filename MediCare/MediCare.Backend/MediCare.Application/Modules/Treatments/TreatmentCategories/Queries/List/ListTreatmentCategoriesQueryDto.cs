namespace MediCare.Application.Modules.Catalog.TreatmentCategories.Queries.List;

public sealed class ListTreatmentCategoriesQueryDto
{
    public required int Id { get; init; }
    public required string CategoryName { get; init; }
    public required bool isEnabled { get; init; }
}
