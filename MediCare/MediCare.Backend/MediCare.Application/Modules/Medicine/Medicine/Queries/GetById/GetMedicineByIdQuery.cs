namespace MediCare.Application.Modules.Medicine.Medicine.Queries.GetById;

public class GetMedicineByIdQuery : IRequest<GetMedicineByIdQueryDto>
{
    public int Id { get; set; }
}