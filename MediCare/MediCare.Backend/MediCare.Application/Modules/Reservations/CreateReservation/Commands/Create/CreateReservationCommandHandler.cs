using MediatR;
using MediCare.Application.Abstractions;
using MediCare.Domain.Entities.HospitalRecords;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Application.Modules.Reservations.CreateReservation.Commands.Create
{
    public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, int>
    {
        private readonly IAppDbContext _context;

        public CreateReservationCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
        {
            // 1. Tretman isti
            var treatment = await _context.Treatments.FirstOrDefaultAsync(t => t.Id == request.TreatmentId, cancellationToken);
            if (treatment == null) throw new Exception("Tretman ne postoji.");

            // 2. FIX: DateTime + string → query
            var dateToCheck = request.ReservationDate.Date;
            var cancelledStatusId = await _context.OrderStatus
                .Where(os => os.StatusName == "Cancelled").Select(os => os.Id).FirstOrDefaultAsync(cancellationToken);

            var existingReservation = await _context.Reservations
                .AnyAsync(r => r.TreatmentId == request.TreatmentId
                            && r.ReservationDate.Date == dateToCheck  // ✅ SAMO OVO!
                            && r.ReservationTime == request.ReservationTime
                            && r.OrderStatusId != cancelledStatusId,
                        cancellationToken);

            if (existingReservation) throw new Exception("Ovaj termin je već zauzet.");

            // 3-4. OSTALO IDENTIČNO
            var draftStatus = await _context.OrderStatus.FirstOrDefaultAsync(os => os.StatusName == "Draft", cancellationToken);
            if (draftStatus == null) throw new Exception("OrderStatus 'Draft' ne postoji u bazi.");

            var reservation = new Domain.Entities.HospitalRecords.Reservations
            {
                UserId = request.UserId,
                TreatmentId = request.TreatmentId,
                ReservationDate = request.ReservationDate,
                ReservationTime = request.ReservationTime,
                OrderStatusId = draftStatus.Id,
                Price = treatment.Price
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync(cancellationToken);
            return reservation.Id;
        }

    }
}
