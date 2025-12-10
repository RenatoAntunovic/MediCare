using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Domain.Common;

namespace MediCare.Domain.Entities.Catalog
{
    public class Treatments : BaseEntity
    {
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public int TreatmentCategoryId { get; set; }
        public TreatmentCategories TreatmentCategory { get; set; }
        public bool isEnabled { get; set; }
    }
}
