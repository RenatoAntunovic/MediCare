using MediatR;

namespace MediCare.Application.Modules.Reservations.Queries.GetById
{
    public class GetReservationByIdQuery : IRequest<ReservationDetailDto>
    {
        public int ReservationId { get; set; }
        public int UserId { get; set; }
    }
}
