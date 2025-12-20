using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Sales.Orders.Commands.Update
{
    public class UpdateOrderCommand:IRequest<int>
    {
        [JsonIgnore]
        public int Id { get; set; }
        public List<UpdateOrderCommandItem> Items { get; set; } = [];
    }

    public class UpdateOrderCommandItem
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public int Quantity { get; set; }
    }
}
