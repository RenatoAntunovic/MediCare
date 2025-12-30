using MediCare.Application.Modules.Medicine.MedicineCategories.Queries.GetById;

public sealed class GetMedicineCategoryByIdQueryValidator : AbstractValidator<GetMedicineCategoryByIdQuery>
{
    public GetMedicineCategoryByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id must be a positive value.");
    }
}