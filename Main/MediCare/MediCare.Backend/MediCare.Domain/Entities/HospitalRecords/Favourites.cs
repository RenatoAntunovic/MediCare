using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Domain.Common;

namespace MediCare.Domain.Entities.HospitalRecords
{
    public class Favourites : BaseEntity
    {
        public int UserId { get; set; }
        public Users User { get; set; }
        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; }
    }
}
