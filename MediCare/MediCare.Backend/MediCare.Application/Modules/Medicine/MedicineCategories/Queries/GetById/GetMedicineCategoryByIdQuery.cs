namespace MediCare.Application.Modules.Medicine.MedicineCategories.Queries.GetById;

public class GetMedicineCategoryByIdQuery : IRequest<GetMedicineCategoryByIdQueryDto>
{
    public int Id { get; set; }
}