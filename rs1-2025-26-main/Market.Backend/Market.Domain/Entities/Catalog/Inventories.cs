using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Market.Domain.Common;

namespace Market.Domain.Entities.Catalog
{
    public class Inventories : BaseEntity
    {
        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; }
        public int QuantityInStock { get; set; }

        public List<ReceivingItems> ReceivingItems { get; set; }
    }
}
