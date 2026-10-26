namespace MediCare.Application.Modules.Sales.Orders.Commands.Create;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("An order must have at least one item.")
            .Must(items => items.Select(i => i.MedicineId).Distinct().Count() == items.Count)
            .WithMessage("The same medicine can't appear twice in one order.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.MedicineId).GreaterThan(0);
            item.RuleFor(i => i.Quantity).InclusiveBetween(1, 100).WithMessage("Quantity must be between 1 and 100.");
        });
    }
}