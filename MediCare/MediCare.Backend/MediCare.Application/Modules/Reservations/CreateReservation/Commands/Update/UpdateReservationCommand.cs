namespace MediCare.Application.Modules.Reservations.Commands.Update
{
    public class UpdateReservationCommand : IRequest<UpdateReservationResponse>
    {
        [JsonIgnore]
        public int ReservationId { get;set; }
        [JsonIgnore]
        public int UserId { get; set; }
        public int TreatmentId { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
    }
}
