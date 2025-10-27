using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Market.Domain.Common;

namespace Market.Domain.Entities.Catalog
{
    public class ReceivingItems : BaseEntity
    {
        public int ReceivingId { get; set; }
        public Receivings Receiving { get; set; }
        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; }
        public int Quantity { get; set; }
        public int InventoryId { get; set; }
        public Inventories Inventory { get; set; }
    }
}
