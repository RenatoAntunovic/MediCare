using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Domain.Common;

namespace MediCare.Domain.Entities.Catalog
{
    public class Medicine : BaseEntity
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int MedicineCategoryId { get; set; }
        public MedicineCategories MedicineCategory { get; set; }
        public string ImagePath { get; set; }
        public int Weight { get; set; }

        public ICollection<ReceivingItems> ReceivingItems { get; set; } = new List<ReceivingItems>();

        public static class Constraints
        {
            public const int NameMaxLength = 150;

            public const int DescriptionMaxLength = 1000;
        }
    }
}
