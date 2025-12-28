using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.ForLater.Command.Delete
{
    public class DeleteForLaterCommand : IRequest<Unit>
    {
        public required int Id { get; set; }
    }
}
