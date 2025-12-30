namespace MediCare.Application.Modules.Catalog.Treatments.Queries.List;

public sealed class ListTreatmentsQueryDto
{
    public required int Id { get; init; }
    public required string ServiceName { get; init; }
    public required decimal Price { get; init; }
    public required string Description { get; init; }
    public required int TreatmentsCategoryId { get; init; }
    public required string TreatmentsCategoryName { get; init; }
    public required string ImagePath { get; init; }
    public required bool isEnabled { get; init; }
}
