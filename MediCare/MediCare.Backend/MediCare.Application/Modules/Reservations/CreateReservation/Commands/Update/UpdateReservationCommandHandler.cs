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
                .FirstOrDefaultAsync(r => r.Id == request.ReservationId && r.UserId == request.UserId, cancellationToken);

            if (reservation == null)
                throw new Exception("Rezervacija ne postoji ili nemate pristup.");

            if (reservation.OrderStatus.StatusName != "DRAFT")
                throw new Exception("Možete mijenjati samo rezervacije sa statusom DRAFT.");

            var treatment = await _context.Treatments
                .FirstOrDefaultAsync(t => t.Id == request.TreatmentId, cancellationToken);

            if (treatment == null)
                throw new Exception("Tretman ne postoji.");

            reservation.TreatmentId = request.TreatmentId;
            reservation.ReservationDate = request.ReservationDate;
            reservation.ReservationTime = request.ReservationTime;
            reservation.Price = treatment.Price;
            reservation.ModifiedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return new UpdateReservationResponse
            {
                Id = reservation.Id,
                Message = "Rezervacija je uspješno ažurirana."
            };
        }
    }
}
