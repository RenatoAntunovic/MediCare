using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Cart.Command.AddToCart
{
    public class AddToCartCommand : IRequest<int>
    {
        public int MedicineId { get; set; }
        public int Quantity { get; set; }
    }
}
