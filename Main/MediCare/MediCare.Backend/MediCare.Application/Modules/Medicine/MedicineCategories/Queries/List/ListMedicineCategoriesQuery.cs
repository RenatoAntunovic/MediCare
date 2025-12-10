namespace MediCare.Application.Modules.Medicine.MedicineCategories.Queries.List;

public sealed class ListMedicineCategoriesQuery : BasePagedQuery<ListMedicineCategoriesQueryDto>
{
    public string? Search { get; init; }
    public bool? OnlyEnabled { get; init; }
}
