using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public sealed class AddToCartFromFavouritesCommandValidator : AbstractValidator<AddToCartFromFavouritesCommand>
{
    public AddToCartFromFavouritesCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.FavouriteId).GreaterThan(0).WithMessage("Favourite must be selected.");
        RuleFor(x => x.Quantity).InclusiveBetween(1, 100).WithMessage("Quantity must be between 1 and 100.");
    }
}
