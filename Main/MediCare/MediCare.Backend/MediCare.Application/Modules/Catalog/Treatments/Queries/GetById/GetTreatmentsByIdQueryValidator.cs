using MediCare.Application.Modules.Catalog.Treatments.Queries.GetById;

public sealed class GetTreatmentsByIdQueryValidator : AbstractValidator<GetTreatmentsByIdQuery>
{
    public GetTreatmentsByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id must be a positive value.");
    }
}