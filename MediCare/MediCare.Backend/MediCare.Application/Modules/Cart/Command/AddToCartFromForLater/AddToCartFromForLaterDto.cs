using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Cart.Command.AddToCartFromForLater
{
    public class AddToCartFromForLaterDto
    {
        public int ForLaterId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
