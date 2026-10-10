using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Favourites.Command.Delete;

public sealed class DeleteFavouritesCommandValidator : AbstractValidator<DeleteFavouritesCommand>
{
    public DeleteFavouritesCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
