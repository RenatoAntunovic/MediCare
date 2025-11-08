using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Domain.Common;

namespace MediCare.Domain.Entities.Catalog
{
    public class ReservationReviews : BaseEntity
    {
        public int UserId { get; set; }
        public Users User { get; set; }
        public int ReservationId { get; set; }
        public Reservations Reservation { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } 
        public DateTime ReviewDate { get; set; }

    }
}
