using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.ForLater.Command.Delete;

public sealed class DeleteForLaterCommandValidator : AbstractValidator<DeleteForLaterCommand>
{
    public DeleteForLaterCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
