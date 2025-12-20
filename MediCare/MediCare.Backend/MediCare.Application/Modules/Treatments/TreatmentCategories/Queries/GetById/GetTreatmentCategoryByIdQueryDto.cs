namespace MediCare.Application.Modules.Catalog.TreatmentCategories.Queries.GetById;

public class GetTreatmentCategoryByIdQueryDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required bool IsEnabled { get; init; }
}
