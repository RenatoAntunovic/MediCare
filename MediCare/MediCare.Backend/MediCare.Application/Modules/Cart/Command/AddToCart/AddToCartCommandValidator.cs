using MediCare.Application.Modules.Cart.Common;
namespace MediCare.Application.Modules.Cart.Command.AddToCart;

public sealed class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.MedicineId)
            .GreaterThan(0).WithMessage("Medicine must be selected.");

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, CartRules.MaxQuantityPerMedicine).WithMessage($"Quantity must be between 1 and {CartRules.MaxQuantityPerMedicine}.");
    }
}