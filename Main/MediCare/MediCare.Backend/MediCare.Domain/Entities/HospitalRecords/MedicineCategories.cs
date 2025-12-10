using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Domain.Common;

namespace MediCare.Domain.Entities.Catalog
{
    public class MedicineCategories : BaseEntity
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public ICollection<Medicine> Medicine { get; set; } = new List<Medicine>();
        public static class Constraints
        {
            public const int NameMaxLength = 100;
        }
    }
}
