using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Domain.Common;

namespace MediCare.Domain.Entities.HospitalRecords
{
    public class ProductReviews : BaseEntity
    {
        public int UserId { get; set; }
        public Users User { get; set; }
        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime ReviewDate { get; set; }
    }
}
