using MediatR;

namespace MediCare.Application.Modules.Reservations.CreateReservation.Commands.Create
{
    public class CreateReservationCommand : IRequest<int>
    {
        public int UserId { get; set; }
        public int TreatmentId { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public string? Notes { get; set; }
    }
}
