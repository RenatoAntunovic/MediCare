namespace MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Status.Enable;

public sealed class EnableTreatmentCategoryCommandValidator : AbstractValidator<EnableTreatmentCategoryCommand>
{
    public EnableTreatmentCategoryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
