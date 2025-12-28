using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Cart.Command.AddToCartFromForLater
{
    public class AddToCartFromForLaterCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public int ForLaterId { get; set; }
        public int Quantity { get; set; } = 1;

        public AddToCartFromForLaterCommand(int userId, int forLaterId, int quantity)
        {
            UserId = userId;
            ForLaterId = forLaterId;
            Quantity = quantity;
        }
    }
}
