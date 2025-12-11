using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Domain.Common;

namespace MediCare.Domain.Entities.HospitalRecords
{
    public class OrderStatus : BaseEntity
    {
        public string StatusName { get; set; }
    }
}
