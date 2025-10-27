using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Market.Domain.Common;

namespace Market.Domain.Entities.Catalog
{
    public class Medicine : BaseEntity
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public MedicineCategories MedicineCategory { get; set; }
        public string ImagePath { get; set; }
        public int Weight { get; set; }
    }
}
