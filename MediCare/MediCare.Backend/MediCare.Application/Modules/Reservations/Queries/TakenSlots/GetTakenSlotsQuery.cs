using MediCare.Application.Modules.Reservations.Common;

namespace MediCare.Application.Modules.Reservations.Queries.TakenSlots;

/// <summary>
/// Returns the taken slots ("HH:mm") for a treatment on a given day – used by the calendar.
/// </summary>
public sealed class GetTakenSlotsQuery : IRequest<List<string>>
{
    public int TreatmentId { get; init; }
    public DateTime Date { get; init; }
}

public sealed class GetTakenSlotsQueryValidator : AbstractValidator<GetTakenSlotsQuery>
{
    public GetTakenSlotsQueryValidator()
    {
        RuleFor(x => x.TreatmentId).GreaterThan(0).WithMessage("Tretman je obavezan.");
        RuleFor(x => x.Date).NotEmpty().WithMessage("Datum je obavezan.");
    }
}

public sealed class GetTakenSlotsQueryHandler(IAppDbContext ctx)
    : IRequestHandler<GetTakenSlotsQuery, List<string>>
{
    public async Task<List<string>> Handle(GetTakenSlotsQuery request, CancellationToken ct)
    {
        var day = request.Date.Date;

        var times = await ctx.Reservations
            .AsNoTracking()
            .Where(r =>
                r.TreatmentId == request.TreatmentId &&
                r.ReservationDate.Date == day &&
                r.OrderStatus.StatusName != ReservationRules.CancelledStatus)
            .Select(r => r.ReservationTime)
            .Distinct()
            .ToListAsync(ct);

        return times
            .OrderBy(t => t)
            .Select(t => t.ToString(@"hh\:mm"))
            .ToList();
    }
}
