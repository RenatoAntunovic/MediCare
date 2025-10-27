using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Market.Domain.Common;

namespace Market.Domain.Entities.Catalog
{
    public class Reservations : BaseEntity
    {
        public int UserId { get; set; }
        public Users User { get; set; }
        public int TreatmentId { get; set; }
        public Treatments Treatment {  get; set; }
        public DateTime ReservationDate { get; set; }
        public string Status { get; set; }
    }
}
