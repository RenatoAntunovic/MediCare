using MediCare.Application.Modules.Medicine.Medicine.Commands.Update;
using MediCare.Domain.Entities.HospitalRecords;
using FluentValidation;

public sealed class UpdateMedicineCommandValidator
    : AbstractValidator<UpdateMedicineCommand>
{
    public UpdateMedicineCommandValidator()
    {
        RuleFor(x => x.Id)
           .GreaterThan(0)
           .WithMessage("Id must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(Medicine.Constraints.NameMaxLength)
            .WithMessage($"Name can be at most {Medicine.Constraints.NameMaxLength} characters long.");

        RuleFor(x => x.Description)
            .MaximumLength(Medicine.Constraints.DescriptionMaxLength)
            .WithMessage($"Description can be at most {Medicine.Constraints.DescriptionMaxLength} characters long.");

        RuleFor(x => x.MedicineCategoryId)
            .GreaterThan(0)
            .WithMessage("Medicine category is required.");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0.");

        RuleFor(x => x.Weight)
            .GreaterThan(0)
            .WithMessage("Weight must be greater than 0.");

        // ImageFile je OPCIONALNO - može biti null ili prazan
        // Za inline edit - nema slike!
    }
}
