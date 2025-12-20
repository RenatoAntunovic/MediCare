namespace MediCare.Application.Modules.Medicine.Medicine.Queries.List;

public sealed class ListMedicineQuery : BasePagedQuery<ListMedicineQueryDto>
{
    public string? Search { get; init; }
    public int? CategoryId { get; set; }
    public bool? OnlyEnabled { get; init; }
}
