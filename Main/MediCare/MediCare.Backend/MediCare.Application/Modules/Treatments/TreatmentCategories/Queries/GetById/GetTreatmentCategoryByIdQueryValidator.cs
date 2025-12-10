using MediCare.Application.Modules.Catalog.TreatmentCategories.Queries.GetById;

public sealed class GetTreatmentCategoryByIdQueryValidator : AbstractValidator<GetTreatmentCategoryByIdQuery>
{
    public GetTreatmentCategoryByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id must be a positive value.");
    }
}