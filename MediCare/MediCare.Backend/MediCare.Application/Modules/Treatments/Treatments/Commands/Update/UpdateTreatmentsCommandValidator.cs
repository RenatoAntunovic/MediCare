using MediCare.Application.Modules.Catalog.Treatments.Commands.Update;

public sealed class UpdateTreatmentsCommandValidator
    : AbstractValidator<UpdateTreatmentsCommand>
{
    public UpdateTreatmentsCommandValidator()
    {
        RuleFor(x => x.Id)
           .GreaterThan(0)
           .WithMessage("Id must be greater than 0.");

        RuleFor(x => x.ServiceName)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(Medicine.Constraints.NameMaxLength)
            .WithMessage($"Name can be at most {Medicine.Constraints.NameMaxLength} characters long.");

        
    }
}