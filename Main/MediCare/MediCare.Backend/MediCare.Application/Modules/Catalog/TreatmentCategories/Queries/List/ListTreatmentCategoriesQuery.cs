namespace MediCare.Application.Modules.Catalog.TreatmentCategories.Queries.List;

public sealed class ListTreatmentCategoriesQuery : BasePagedQuery<ListTreatmentCategoriesQueryDto>
{
    public string? Search { get; init; }
    public bool? OnlyEnabled { get; init; }
}
