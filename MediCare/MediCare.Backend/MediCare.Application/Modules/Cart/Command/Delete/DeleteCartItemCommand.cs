using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Cart.Command.Delete
{
    public class DeleteCartItemCommand : IRequest<Unit>
    {
        public required int Id { get; set; }
    }
}
