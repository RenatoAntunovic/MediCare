using System;
using System.ComponentModel.DataAnnotations;
using MediCare.Domain.Common;

namespace MediCare.Domain.Entities.HospitalRecords
{
    public class Reservations : BaseEntity
    {
        public int UserId { get; set; }
        public Users User { get; set; }
        public int TreatmentId { get; set; }
        public Treatments Treatment { get; set; }

        /// <summary>Date only (no time component).</summary>
        public DateTime ReservationDate { get; set; }

        /// <summary>Appointment time, e.g. 09:30.</summary>
        public TimeSpan ReservationTime { get; set; }

        public int OrderStatusId { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public decimal Price { get; set; }

        /// <summary>Description of the problem entered by the patient when booking.</summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
