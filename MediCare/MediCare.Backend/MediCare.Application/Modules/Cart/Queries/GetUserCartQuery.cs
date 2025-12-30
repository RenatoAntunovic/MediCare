using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Cart.Queries
{
    public class GetUserCartQuery:IRequest<UserCartDto>
    {
    }

    public class UserCartDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalPrice => Items.Sum(i => i.Price);
    }

    public class CartItemDto
    {
        public int CartItemId { get; set; }
        public int MedicineId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
    }
}
