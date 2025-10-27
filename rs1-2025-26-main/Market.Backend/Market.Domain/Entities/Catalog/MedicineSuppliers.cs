using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Market.Domain.Common;

namespace Market.Domain.Entities.Catalog
{
    public class MedicineSuppliers : BaseEntity
    {
        public int SupplierId { get; set; }
        public Suppliers Supplier { get; set; }
        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; }
    }
}
