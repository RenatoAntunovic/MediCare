namespace MediCare.Application.Modules.Catalog.TreatmentCategories.Queries.GetById;

public class GetTreatmentCategoryByIdQuery : IRequest<GetTreatmentCategoryByIdQueryDto>
{
    public int Id { get; set; }
}