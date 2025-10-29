namespace MediCare.Application.Modules.Catalog.ProductCategories.Queries.GetById;

public class GetMedicineCategoryByIdQuery : IRequest<GetMedicineCategoryByIdQueryDto>
{
    public int Id { get; set; }
}