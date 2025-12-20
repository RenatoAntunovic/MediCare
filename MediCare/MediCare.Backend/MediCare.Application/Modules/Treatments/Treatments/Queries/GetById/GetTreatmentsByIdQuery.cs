namespace MediCare.Application.Modules.Catalog.Treatments.Queries.GetById;

public class GetTreatmentsByIdQuery : IRequest<GetTreatmentsByIdQueryDto>
{
    public int Id { get; set; }
}