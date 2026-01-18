using MediatR;

namespace MediCare.Application.Modules.Reservations.Commands.Status.ChangeStatus
{
    public class ChangeReservationStatusCommand : IRequest
    {
        public int Id { get; set; }
        public int NewStatusId { get; set; }
    }
}
