using MediCare.Application.Modules.Catalog.Medicine.Queries.GetById;

public sealed class GetMedicineByIdQueryValidator : AbstractValidator<GetMedicineByIdQuery>
{
    public GetMedicineByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id must be a positive value.");
    }
}