using MediCare.Application.Modules.Reservations.Common;
using ReservationEntity = MediCare.Domain.Entities.HospitalRecords.Reservations;

namespace MediCare.Application.Modules.Reservations.CreateReservation.Commands.Create;

public sealed class CreateReservationCommandHandler(IAppDbContext ctx)
    : IRequestHandler<CreateReservationCommand, int>
{
    public async Task<int> Handle(CreateReservationCommand request, CancellationToken ct)
    {
        var date = request.ReservationDate.Date;

        var treatment = await ctx.Treatments
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TreatmentId, ct)
            ?? throw new MediCareNotFoundException("Tretman ne postoji.");

        if (!treatment.isEnabled)
            throw new MediCareBusinessRuleException(
                "reservation.treatment.disabled",
                "Ovaj tretman trenutno nije dostupan za rezervaciju.");

        var isTaken = await ctx.Reservations.AnyAsync(r =>
                r.TreatmentId == request.TreatmentId &&
                r.ReservationDate.Date == date &&
                r.ReservationTime == request.ReservationTime &&
                r.OrderStatus.StatusName != ReservationRules.CancelledStatus,
            ct);

        if (isTaken)
            throw new MediCareConflictException("Ovaj termin je upravo zauzet. Odaberite drugi termin.");

        var draftStatusId = await ctx.OrderStatus
            .Where(s => s.StatusName == ReservationRules.DraftStatus)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new MediCareBusinessRuleException(
                "reservation.status.missing",
                "Status DRAFT ne postoji u bazi. Pokrenite seed podataka.");

        var reservation = new ReservationEntity
        {
            UserId = request.UserId,
            TreatmentId = request.TreatmentId,
            ReservationDate = date,                 // store the date ONLY
            ReservationTime = request.ReservationTime,
            OrderStatusId = draftStatusId,
            Price = treatment.Price,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };

        ctx.Reservations.Add(reservation);
        await ctx.SaveChangesAsync(ct);

        return reservation.Id;
    }
}
