using MediatR;
using MediCare.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Application.Modules.Reservations.Queries.GetById
{
    public class GetReservationByIdQueryHandler : IRequestHandler<GetReservationByIdQuery, ReservationDetailDto>
    {
        private readonly IAppDbContext _context;

        public GetReservationByIdQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ReservationDetailDto> Handle(GetReservationByIdQuery request, CancellationToken cancellationToken)
        {
            var reservation = await _context.Reservations
                .Where(r => r.Id == request.ReservationId && r.UserId == request.UserId)
                .Include(r => r.Treatment)
                .Include(r => r.OrderStatus)
                .Select(r => new ReservationDetailDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    TreatmentId = r.TreatmentId,
                    TreatmentName = r.Treatment.ServiceName,
                    TreatmentDescription = r.Treatment.Description,
                    ReservationDate = r.ReservationDate,
                    ReservationTime = r.ReservationTime,
                    OrderStatus = r.OrderStatus.StatusName,
                    Price = r.Price
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (reservation == null)
                throw new Exception("Rezervacija ne postoji ili nemate pristup.");

            return reservation;
        }
    }
}
