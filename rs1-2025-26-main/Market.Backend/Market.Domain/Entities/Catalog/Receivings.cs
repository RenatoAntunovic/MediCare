using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Market.Domain.Common;

namespace Market.Domain.Entities.Catalog
{
    public class Receivings : BaseEntity
    {
        public DateTime ReceivedDate { get; set; }
        public int SupplierId { get; set; }
        public Suppliers Supplier { get; set; } // supplier od kojeg stiže roba

        public List<ReceivingItems> Items { get; set; }
    }
}
