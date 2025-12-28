using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Cart.Command.Checkout
{
    public class CheckoutOrderResponseDto
    {
        public int OrderId { get; set; }
        public decimal TotalPrice { get; set; }
        public string UserFcmToken { get; set; }
    }
}
