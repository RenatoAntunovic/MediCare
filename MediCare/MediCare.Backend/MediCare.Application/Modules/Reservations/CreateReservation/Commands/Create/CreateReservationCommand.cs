namespace MediCare.Application.Modules.Reservations.CreateReservation.Commands.Create
{
    public class CreateReservationCommand : IRequest<int>
    {
        /// <summary>Set by the controller from the JWT – the client cannot send it.</summary>
        [JsonIgnore]
        public int UserId { get; set; }

        public int TreatmentId { get; set; }

        /// <summary>Date only, format "yyyy-MM-dd".</summary>
        public DateTime ReservationDate { get; set; }

        /// <summary>Format "HH:mm:ss", e.g. "09:30:00".</summary>
        public TimeSpan ReservationTime { get; set; }

        public string? Notes { get; set; }
    }
}
