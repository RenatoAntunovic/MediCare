using MediCare.Application.Modules.Cart.Common;
namespace MediCare.Application.Modules.Cart.Command.SetQuantity;

public sealed class SetCartItemQuantityCommandValidator : AbstractValidator<SetCartItemQuantityCommand>
{
    public SetCartItemQuantityCommandValidator()
    {
        RuleFor(x => x.CartItemId).GreaterThan(0);
        RuleFor(x => x.Quantity).InclusiveBetween(1, CartRules.MaxQuantityPerMedicine).WithMessage($"Quantity must be between 1 and {CartRules.MaxQuantityPerMedicine}.");
    }
}