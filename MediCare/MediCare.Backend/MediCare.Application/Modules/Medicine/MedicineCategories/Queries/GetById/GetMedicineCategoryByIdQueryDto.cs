namespace MediCare.Application.Modules.Medicine.MedicineCategories.Queries.GetById;

public class GetMedicineCategoryByIdQueryDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required bool IsEnabled { get; init; }
}
