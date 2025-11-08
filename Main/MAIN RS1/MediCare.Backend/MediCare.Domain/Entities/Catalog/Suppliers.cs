using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Domain.Common;

namespace MediCare.Domain.Entities.Catalog
{
    public class Suppliers : BaseEntity
    {
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; } 
    }
}
