using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Domain.Common;

namespace MediCare.Domain.Entities.Catalog
{
    public class TreatmentCategories : BaseEntity
    {
        public string CategoryName { get; set; }
        public bool isEnabled { get; set; }
        public ICollection<Treatments> Treatments { get; set; } = new List<Treatments>();

        public static class Constraints
        {
            public const int NameMaxLength = 100;
        }
    }
}
