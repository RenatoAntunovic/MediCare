using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Sales.Orders.Commands.Create
{
    public class CreateOrderCommand : IRequest<int>
    {
        public int UserId { get; set; }
        public List<CreateOrderCommandItem> Items { get; set; } = [];
    }

    public class CreateOrderCommandItem
    {
        public int MedicineId { get; set; }
        public int Quantity { get; set; }
    }
}
