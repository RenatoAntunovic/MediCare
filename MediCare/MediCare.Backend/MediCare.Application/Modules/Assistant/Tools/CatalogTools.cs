using MediCare.Application.Modules.Reservations.Common;

namespace MediCare.Application.Modules.Assistant.Tools;

/// <summary>Read-only catalog and availability lookups shared by the patient and admin assistant.</summary>
internal sealed class CatalogTools(IAppDbContext db)
{
    public const int MaxSlotDays = 10;

    public static readonly AiTool ListTreatmentsTool = new(
        "list_treatments",
        "List all bookable treatments (medical services / appointments) with id, name, category, price and description.",
        ToolSchemas.None);

    public static readonly AiTool GetFreeSlotsTool = new(
        "get_free_slots",
        "Free appointment times for one treatment. Appointments are on working days 08:00-17:00 every 30 min, from tomorrow on.",
        """
        {"type":"object","properties":{
          "treatment_id":{"type":"integer","description":"Id from list_treatments"},
          "from":{"type":"string","description":"First day to check, yyyy-MM-dd (default: tomorrow)"},
          "days":{"type":"integer","description":"How many working days to check, 1-10 (default 5)"}},
         "required":["treatment_id"]}
        """);

    public async Task<object> ListTreatmentsAsync(CancellationToken ct) =>
        await db.Treatments.AsNoTracking()
            .Where(t => t.isEnabled)
            .OrderBy(t => t.ServiceName)
            .Select(t => new
            {
                id = t.Id,
                name = t.ServiceName,
                category = t.TreatmentCategory.CategoryName,
                price = t.Price,
                description = t.Description
            })
            .ToListAsync(ct);

    public async Task<object> GetFreeSlotsAsync(System.Text.Json.JsonElement args, CancellationToken ct)
    {
        var treatmentId = ToolArgs.Int(args, "treatment_id");
        var treatment = treatmentId is null
            ? null
            : await db.Treatments.AsNoTracking().FirstOrDefaultAsync(t => t.Id == treatmentId && t.isEnabled, ct);
        if (treatment is null)
            return new { error = "Unknown treatment_id – call list_treatments first." };

        var tomorrow = DateTime.Today.AddDays(1);
        var day = ToolArgs.Date(args, "from") is { } from && from > tomorrow ? from : tomorrow;
        var daysWanted = Math.Clamp(ToolArgs.Int(args, "days") ?? 5, 1, MaxSlotDays);

        var days = new List<DateTime>();
        while (days.Count < daysWanted)
        {
            if (ReservationRules.IsBookableDate(day))
                days.Add(day);
            day = day.AddDays(1);
        }

        var first = days[0];
        var lastExclusive = days[^1].AddDays(1);
        var taken = (await db.Reservations.AsNoTracking()
                .Where(r => r.TreatmentId == treatment.Id
                            && r.ReservationDate >= first
                            && r.ReservationDate < lastExclusive
                            && r.OrderStatus.StatusName != ReservationRules.CancelledStatus)
                .Select(r => new { r.ReservationDate, r.ReservationTime })
                .ToListAsync(ct))
            .Select(r => (r.ReservationDate.Date, r.ReservationTime))
            .ToHashSet();

        return new
        {
            treatmentId = treatment.Id,
            treatment = treatment.ServiceName,
            price = treatment.Price,
            days = days.Select(d => new
            {
                date = ToolText.Day(d),
                weekday = d.DayOfWeek.ToString(),
                free = AllSlots()
                    .Where(t => !taken.Contains((d, t)))
                    .Select(ToolText.Time)
                    .ToList()
            })
        };
    }

    public static IEnumerable<TimeSpan> AllSlots()
    {
        for (var t = ReservationRules.FirstSlot; t <= ReservationRules.LastSlot; t += ReservationRules.SlotLength)
            yield return t;
    }
}
