using MediCare.Application.Modules.Catalog.ProductCategories.Queries.GetById;

public sealed class GetMedicineCategoryByIdQueryValidator : AbstractValidator<GetMedicineCategoryByIdQuery>
{
    public GetMedicineCategoryByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id must be a positive value.");
    }
}