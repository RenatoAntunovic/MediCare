namespace MediCare.Application.Modules.Catalog.MedicineCategories.Queries.GetById;

public class GetMedicineCategoryByIdQuery : IRequest<GetMedicineCategoryByIdQueryDto>
{
    public int Id { get; set; }
}