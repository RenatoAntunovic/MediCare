using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Cart.Command.Checkout
{
    public class CheckoutOrderCommand : IRequest<CheckoutOrderResponseDto>
    {
        public int UserId { get; set; }
    }
}
