using MediCare.Application.Modules.Reservations.Common;

namespace MediCare.Application.Modules.Reservations.Commands.Update
{
    public class UpdateReservationCommandHandler : IRequestHandler<UpdateReservationCommand, UpdateReservationResponse>
    {
        private readonly IAppDbContext _context;

        public UpdateReservationCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<UpdateReservationResponse> Handle(UpdateReservationCommand request, CancellationToken cancellationToken)
        {
            var reservation = await _context.Reservations
                .Include(r => r.OrderStatus)
                .FirstOrDefaultAsync(r => r.Id == request.ReservationId && r.UserId == request.UserId, cancellationToken)
                ?? throw new MediCareNotFoundException("Rezervacija ne postoji ili nemate pristup.");

            if (reservation.OrderStatus.StatusName != ReservationRules.DraftStatus)
                throw new MediCareBusinessRuleException(
                    "reservation.update.notDraft",
                    "Možete mijenjati samo rezervacije koje još nisu potvrđene.");

            var treatment = await _context.Treatments
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == request.TreatmentId, cancellationToken)
                ?? throw new MediCareNotFoundException("Tretman ne postoji.");

            if (!treatment.isEnabled)
                throw new MediCareBusinessRuleException(
                    "reservation.treatment.disabled",
                    "Ovaj tretman trenutno nije dostupan za rezervaciju.");

            var date = request.ReservationDate.Date;

            // Same check as on create, but excluding this reservation
            var isTaken = await _context.Reservations.AnyAsync(r =>
                    r.Id != reservation.Id &&
                    r.TreatmentId == request.TreatmentId &&
                    r.ReservationDate.Date == date &&
                    r.ReservationTime == request.ReservationTime &&
                    r.OrderStatus.StatusName != ReservationRules.CancelledStatus,
                cancellationToken);

            if (isTaken)
                throw new MediCareConflictException("Odabrani termin je zauzet. Odaberite drugi termin.");

            reservation.TreatmentId = request.TreatmentId;
            reservation.ReservationDate = date;
            reservation.ReservationTime = request.ReservationTime;
            reservation.Price = treatment.Price;

            await _context.SaveChangesAsync(cancellationToken);

            return new UpdateReservationResponse
            {
                Id = reservation.Id,
                Message = "Rezervacija je uspješno ažurirana."
            };
        }
    }
}
