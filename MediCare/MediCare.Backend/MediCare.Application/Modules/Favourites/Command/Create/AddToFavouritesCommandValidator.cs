using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Favourites.Command.Create;

public sealed class AddToFavouritesCommandValidator : AbstractValidator<AddToFavouritesCommand>
{
    public AddToFavouritesCommandValidator()
    {
        RuleFor(x => x.MedicineId).GreaterThan(0).WithMessage("Medicine must be selected.");
    }
}
