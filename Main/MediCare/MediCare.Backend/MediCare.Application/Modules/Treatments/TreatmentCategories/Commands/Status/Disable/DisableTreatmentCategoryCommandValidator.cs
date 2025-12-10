namespace MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Status.Disable;

public sealed class DisableTreatmentCategoryCommandValidator : AbstractValidator<DisableTreatmentCategoryCommand>
{
    public DisableTreatmentCategoryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
