using System.Text.Json;
using MediCare.Application.Modules.Assistant.Commands.Chat;
using MediCare.Application.Modules.Notifications;
using MediCare.Application.Modules.Statistics.Queries.Dashboard;

namespace MediCare.Application.Modules.Assistant.Tools;

/// <summary>
/// Read-only tools for admins: who is booked when, order states, stock and the statistics summary.
/// Nothing here changes data.
///
/// Privacy: every tool result is split in two.
/// - The object a tool RETURNS goes to the external AI service: only counts, dates, times, statuses and ids.
/// - Patient names and notes are put into <see cref="Tables"/>. The handler attaches them to the answer
///   for the admin's screen; they are never part of a message sent to the AI service.
/// </summary>
internal sealed class AdminToolbox(IAppDbContext db, ISender sender) : IAssistantToolbox
{
    private const int MaxRows = 60;

    private readonly CatalogTools _catalog = new(db);

    /// <summary>Tables with personal data, filled by the tools, read by the handler after the final answer.</summary>
    public List<AssistantTableDto> Tables { get; } = [];

    public IReadOnlyList<AiTool> Tools { get; } =
    [
        new("get_reservations",
            "Appointments in a date range (default: today + next 7 days): id, date, time, treatment, status, with counts. " +
            "Patient names and notes are NOT available to you: the app shows them to the admin in a table under your answer.",
            """
            {"type":"object","properties":{
              "from":{"type":"string","description":"yyyy-MM-dd"},
              "to":{"type":"string","description":"yyyy-MM-dd"},
              "treatment_id":{"type":"integer"},
              "status":{"type":"string","enum":["DRAFT","CONFIRMED","PAID","COMPLETED","CANCELLED"]}}}
            """),
        new("get_orders",
            "Pharmacy orders: counts and totals per status plus the latest orders (id, date, total, status). Optional filters. " +
            "Customer names are NOT available to you: the app shows them to the admin in a table under your answer.",
            """
            {"type":"object","properties":{
              "from":{"type":"string","description":"yyyy-MM-dd"},
              "to":{"type":"string","description":"yyyy-MM-dd"},
              "status":{"type":"string","enum":["DRAFT","CONFIRMED","PAID","COMPLETED","CANCELLED"]}}}
            """),
        new("get_stock",
            "Current stock per medicine. only_low=true returns medicines at or below the threshold (default 10).",
            """
            {"type":"object","properties":{
              "only_low":{"type":"boolean"},
              "threshold":{"type":"integer"}}}
            """),
        new("get_statistics",
            "Summary for a period (default: all time): realised revenue (paid only) and the confirmed-not-paid amount " +
            "(medicines/treatments), counts, new users, best-selling medicines, busiest treatments, monthly totals.",
            ToolSchemas.DateRange),
        CatalogTools.ListTreatmentsTool,
        CatalogTools.GetFreeSlotsTool,
    ];

    public async Task<object> ExecuteAsync(string name, JsonElement args, CancellationToken ct) => name switch
    {
        "get_reservations" => await ReservationsAsync(args, ct),
        "get_orders" => await OrdersAsync(args, ct),
        "get_stock" => await StockAsync(args, ct),
        "get_statistics" => await StatisticsAsync(args, ct),
        "list_treatments" => await _catalog.ListTreatmentsAsync(ct),
        "get_free_slots" => await _catalog.GetFreeSlotsAsync(args, ct),
        _ => new { error = $"Unknown tool '{name}'." }
    };

    private async Task<object> ReservationsAsync(JsonElement args, CancellationToken ct)
    {
        var from = ToolArgs.Date(args, "from") ?? DateTime.Today;
        var to = ToolArgs.Date(args, "to") ?? from.AddDays(7);
        if (to < from) (from, to) = (to, from);
        var toExclusive = to.AddDays(1);

        var q = db.Reservations.AsNoTracking()
            .Where(r => r.ReservationDate >= from && r.ReservationDate < toExclusive);

        if (ToolArgs.Int(args, "treatment_id") is { } treatmentId)
            q = q.Where(r => r.TreatmentId == treatmentId);
        if (ToolArgs.String(args, "status") is { } status)
            q = q.Where(r => r.OrderStatus.StatusName == status);

        var total = await q.CountAsync(ct);
        var byStatus = await q
            .GroupBy(r => r.OrderStatus.StatusName)
            .Select(g => new { status = g.Key, count = g.Count() })
            .ToListAsync(ct);
        var rows = await q
            .OrderBy(r => r.ReservationDate).ThenBy(r => r.ReservationTime)
            .Take(MaxRows)
            .Select(r => new
            {
                r.Id,
                r.ReservationDate,
                r.ReservationTime,
                Patient = r.User.FirstName + " " + r.User.LastName,
                Treatment = r.Treatment.ServiceName,
                Status = r.OrderStatus.StatusName,
                r.Notes
            })
            .ToListAsync(ct);

        // Display part: names and notes go to the admin's screen only
        Tables.Add(new AssistantTableDto
        {
            Title = $"Rezervacije {from:dd.MM.yyyy} – {to:dd.MM.yyyy}",
            Columns = ["Datum", "Vrijeme", "Pacijent", "Tretman", "Status", "Napomena"],
            Rows = rows.Select(r => new List<string>
            {
                r.ReservationDate.ToString("dd.MM.yyyy"),
                ToolText.Time(r.ReservationTime),
                r.Patient,
                r.Treatment,
                StatusLabels.ToBosnian(r.Status),
                r.Notes ?? string.Empty
            }).ToList()
        });

        // Model part: no patient, no notes
        return new
        {
            from = ToolText.Day(from),
            to = ToolText.Day(to),
            total,
            shown = rows.Count,
            byStatus,
            reservations = rows.Select(r => new
            {
                id = r.Id,
                date = ToolText.Day(r.ReservationDate),
                time = ToolText.Time(r.ReservationTime),
                treatment = r.Treatment,
                status = r.Status
            }),
            note = "Patient names and notes are not available to you. The app shows them to the admin in a table under your answer."
        };
    }

    private async Task<object> OrdersAsync(JsonElement args, CancellationToken ct)
    {
        var q = db.Orders.AsNoTracking();

        var from = ToolArgs.Date(args, "from");
        var to = ToolArgs.Date(args, "to");
        if (from.HasValue) q = q.Where(o => o.OrderDate >= from.Value);
        if (to.HasValue)
        {
            var toExclusive = to.Value.AddDays(1);
            q = q.Where(o => o.OrderDate < toExclusive);
        }

        var byStatus = await q
            .GroupBy(o => o.OrderStatus.StatusName)
            .Select(g => new { status = g.Key, count = g.Count(), total = g.Sum(o => o.TotalPrice) })
            .ToListAsync(ct);

        if (ToolArgs.String(args, "status") is { } status)
            q = q.Where(o => o.OrderStatus.StatusName == status);

        var latest = await q
            .OrderByDescending(o => o.OrderDate)
            .Take(20)
            .Select(o => new
            {
                o.Id,
                o.OrderDate,
                Customer = o.User.FirstName + " " + o.User.LastName,
                o.TotalPrice,
                Status = o.OrderStatus.StatusName,
                ItemsCount = o.OrderItems.Sum(i => i.Quantity)
            })
            .ToListAsync(ct);

        // Display part: customer names go to the admin's screen only
        Tables.Add(new AssistantTableDto
        {
            Title = "Posljednje narudžbe",
            Columns = ["Narudžba", "Datum", "Kupac", "Iznos", "Status", "Stavki"],
            Rows = latest.Select(o => new List<string>
            {
                $"#{o.Id}",
                o.OrderDate.ToString("dd.MM.yyyy"),
                o.Customer,
                $"{o.TotalPrice:0.00} KM",
                StatusLabels.ToBosnian(o.Status),
                o.ItemsCount.ToString()
            }).ToList()
        });

        // Model part: no customer
        return new
        {
            byStatus,
            latest = latest.Select(o => new
            {
                id = o.Id,
                date = ToolText.Day(o.OrderDate),
                total = o.TotalPrice,
                status = o.Status,
                items = o.ItemsCount
            }),
            note = "Customer names are not available to you. The app shows them to the admin in a table under your answer."
        };
    }

    private async Task<object> StockAsync(JsonElement args, CancellationToken ct)
    {
        var threshold = Math.Clamp(ToolArgs.Int(args, "threshold") ?? 10, 0, 1000);
        var onlyLow = ToolArgs.Bool(args, "only_low") ?? false;

        var rows = await db.Medicine.AsNoTracking()
            .Select(m => new
            {
                name = m.Name,
                enabled = m.isEnabled,
                stock = db.Inventories.Where(i => i.MedicineId == m.Id).Select(i => (int?)i.QuantityInStock).FirstOrDefault()
            })
            .OrderBy(m => m.stock)
            .ToListAsync(ct);

        return new
        {
            threshold,
            medicines = rows.Where(m => !onlyLow || (m.stock ?? 0) <= threshold).Take(MaxRows)
        };
    }

    private async Task<object> StatisticsAsync(JsonElement args, CancellationToken ct)
    {
        var s = await sender.Send(new StatisticsDashboardQuery
        {
            From = ToolArgs.Date(args, "from"),
            To = ToolArgs.Date(args, "to")
        }, ct);

        return new
        {
            s.From,
            s.To,
            // revenue = paid only; the confirmed amount is reported apart (same rule as the statistics page)
            s.OrdersRevenue,
            s.ReservationsRevenue,
            totalRevenue = s.OrdersRevenue + s.ReservationsRevenue,
            s.OrdersConfirmedAmount,
            s.ReservationsConfirmedAmount,
            s.OrdersCount,
            s.ReservationsCount,
            s.NewUsersCount,
            s.LowStockCount,
            s.OrdersByStatus,
            s.ReservationsByTreatment,
            s.ReservationsByWeekday,
            s.TopMedicines,
            byMonth = s.ByMonth.Select(m => new { month = $"{m.Year}-{m.Month:00}", m.OrdersRevenue, m.ReservationsRevenue, m.OrdersCount, m.ReservationsCount })
        };
    }
}
