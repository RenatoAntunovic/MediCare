using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Market.Domain.Common;

namespace Market.Domain.Entities.Catalog
{
    public class OrderItems : BaseEntity
    {
        public int OrderId { get; set; }
        public Orders Order { get; set; }
        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
