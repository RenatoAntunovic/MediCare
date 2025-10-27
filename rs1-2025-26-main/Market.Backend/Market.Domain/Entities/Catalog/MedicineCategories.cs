using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Market.Domain.Common;

namespace Market.Domain.Entities.Catalog
{
    public class MedicineCategories : BaseEntity
    {
        public string Name { get; set; }
    }
}
