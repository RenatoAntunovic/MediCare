namespace MediCare.Application.Modules.Catalog.Medicine.Queries.List;

public sealed class ListMedicineQuery : BasePagedQuery<ListMedicineQueryDto>
{
    public string? Search { get; init; }
    public bool? OnlyEnabled { get; init; }
}
