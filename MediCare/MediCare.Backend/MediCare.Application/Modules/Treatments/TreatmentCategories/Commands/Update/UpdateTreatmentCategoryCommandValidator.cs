using MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Update;

public sealed class UpdateTreatmentCategoryCommandValidator
    : AbstractValidator<UpdateTreatmentCategoryCommand>
{
    public UpdateTreatmentCategoryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(TreatmentCategories.Constraints.NameMaxLength).WithMessage($"Name can be at most {TreatmentCategories.Constraints.NameMaxLength} characters long.");
    }
}