using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.ForLater.Command.Create;

public sealed class AddToForLaterCommandValidator : AbstractValidator<AddToForLaterCommand>
{
    public AddToForLaterCommandValidator()
    {
        RuleFor(x => x.MedicineId).GreaterThan(0).WithMessage("Medicine must be selected.");
    }
}
