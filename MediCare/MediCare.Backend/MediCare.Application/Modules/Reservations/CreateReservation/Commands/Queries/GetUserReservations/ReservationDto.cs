namespace MediCare.Application.Modules.Reservations.CreateReservation.Queries.GetReservations
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TreatmentId { get; set; }
        public string TreatmentName { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public string OrderStatus { get; set; }
        public decimal Price { get; set; }
    }
}
