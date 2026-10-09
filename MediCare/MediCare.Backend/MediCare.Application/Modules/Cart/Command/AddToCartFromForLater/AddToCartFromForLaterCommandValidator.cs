using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Cart.Command.AddToCartFromForLater;

public sealed class AddToCartFromForLaterCommandValidator : AbstractValidator<AddToCartFromForLaterCommand>
{
    public AddToCartFromForLaterCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.ForLaterId).GreaterThan(0).WithMessage("Saved item must be selected.");
        RuleFor(x => x.Quantity).InclusiveBetween(1, 100).WithMessage("Quantity must be between 1 and 100.");
    }
}