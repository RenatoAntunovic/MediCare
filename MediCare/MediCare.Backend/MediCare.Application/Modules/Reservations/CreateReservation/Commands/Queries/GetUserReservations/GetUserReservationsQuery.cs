using MediatR;

namespace MediCare.Application.Modules.Reservations.CreateReservation.Queries.GetReservations
{
    public class GetUserReservationsQuery : IRequest<List<ReservationDto>>
    {
        public int UserId { get; set; }
    }
}
