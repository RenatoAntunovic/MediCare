namespace MediCare.Application.Modules.Catalog.Treatments.Queries.List;

public sealed class ListTreatmentsQuery : BasePagedQuery<ListTreatmentsQueryDto>
{
    public string? Search { get; init; }
    public bool? OnlyEnabled { get; init; }
}
