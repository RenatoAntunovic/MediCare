namespace MediCare.Application.Modules.Catalog.Medicine.Queries.GetById;

public class GetMedicineByIdQuery : IRequest<GetMedicineByIdQueryDto>
{
    public int Id { get; set; }
}