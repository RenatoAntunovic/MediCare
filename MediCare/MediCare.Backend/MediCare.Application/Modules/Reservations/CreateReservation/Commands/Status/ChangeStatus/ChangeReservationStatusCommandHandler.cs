using MediatR;
using MediCare.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Application.Modules.Reservations.Commands.Status.ChangeStatus
{
    public class ChangeReservationStatusCommandHandler : IRequestHandler<ChangeReservationStatusCommand>
    {
        private readonly IAppDbContext _context;

        public ChangeReservationStatusCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ChangeReservationStatusCommand request, CancellationToken ct)
        {
            var reservation = await _context.Reservations
                .Include(r => r.OrderStatus)
                .FirstOrDefaultAsync(r => r.Id == request.Id, ct)
                ?? throw new KeyNotFoundException($"Rezervacija sa Id {request.Id} ne postoji.");

            var newStatus = await _context.OrderStatus
                .FirstOrDefaultAsync(s => s.Id == request.NewStatusId, ct)
                ?? throw new KeyNotFoundException($"OrderStatus sa Id {request.NewStatusId} ne postoji.");

            reservation.OrderStatusId = newStatus.Id;
            reservation.OrderStatus = newStatus;

            await _context.SaveChangesAsync(ct);
        }
    }
}
