using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Domain.Common;

namespace MediCare.Domain.Entities.Catalog
{
    public class CartItems : BaseEntity
    {
        public int CartId { get; set; }
        public Carts Cart { get; set; }
        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public void SetPriceFromMedicine()
        {
            if (Medicine != null)
                Price = Medicine.Price * Quantity;
        }
    }
}
