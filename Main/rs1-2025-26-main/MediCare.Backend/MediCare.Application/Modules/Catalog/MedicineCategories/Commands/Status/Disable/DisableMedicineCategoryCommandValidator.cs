namespace MediCare.Application.Modules.Catalog.ProductCategories.Commands.Status.Disable;

public sealed class DisableMedicineCategoryCommandValidator : AbstractValidator<DisableMedicineCategoryCommand>
{
    public DisableMedicineCategoryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
