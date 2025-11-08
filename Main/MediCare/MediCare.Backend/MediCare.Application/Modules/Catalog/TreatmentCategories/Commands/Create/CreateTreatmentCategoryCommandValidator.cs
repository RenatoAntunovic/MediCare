using MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Create;

public sealed class CreateTreatmentCategoryCommandValidator
    : AbstractValidator<CreateTreatmentCategoryCommand>
{
    public CreateTreatmentCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(TreatmentCategories.Constraints.NameMaxLength).WithMessage($"Name can be at most {TreatmentCategories.Constraints.NameMaxLength} characters long.");
    }
}