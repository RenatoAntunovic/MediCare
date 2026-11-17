using System.Text.Json;
using MediCare.Application.Modules.Assistant.Commands.Chat;
using MediCare.Application.Modules.Reservations.Common;

namespace MediCare.Application.Modules.Assistant.Tools;

/// <summary>
/// Tools for a logged-in patient: catalog, free appointments, their own orders/reservations,
/// and suggest_actions – the only way an item becomes a button in the UI (validated here).
/// </summary>
internal sealed class PatientToolbox(IAppDbContext db, IMedicineSearchService medicineSearch, int userId) : IAssistantToolbox
{
    private const int MaxSuggestionsPerKind = 3;
    private const int MaxSearchResults = 40;
    private const int MaxSearchWords = 5;

    private readonly CatalogTools _catalog = new(db);

    /// <summary>Filled by suggest_actions; read by the handler after the final answer.</summary>
    public AssistantSuggestionsDto Suggestions { get; private set; } = new();

    public IReadOnlyList<AiTool> Tools { get; } =
    [
        new("search_medicines",
            "Search the pharmacy catalog. Pass symptom or medicine keywords (e.g. 'headache', 'fever', 'Brufen'). " +
            "Returns matching medicines with id, price, description and stock; if nothing matches, it says so (match=false) and returns no medicines.",
            """
            {"type":"object","properties":{
              "query":{"type":"string","description":"Keywords, at least 3 letters"}}}
            """),
        CatalogTools.ListTreatmentsTool,
        CatalogTools.GetFreeSlotsTool,
        new("get_my_reservations", "The patient's own upcoming and recent appointments.", ToolSchemas.None),
        new("get_my_orders", "The patient's own last 10 pharmacy orders with status and items.", ToolSchemas.None),
        new("suggest_actions",
            "Call this BEFORE your final answer whenever you recommend specific medicines, treatments or appointment times, " +
            "so the app can show 'Add to cart' / 'Book' buttons. Use only ids and free times returned by other tools. Max 3 of each.",
            """
            {"type":"object","properties":{
              "medicine_ids":{"type":"array","items":{"type":"integer"}},
              "treatment_ids":{"type":"array","items":{"type":"integer"},"description":"Treatments to book without a specific time"},
              "slots":{"type":"array","items":{"type":"object","properties":{
                  "treatment_id":{"type":"integer"},
                  "date":{"type":"string","description":"yyyy-MM-dd"},
                  "time":{"type":"string","description":"HH:mm"}},
                "required":["treatment_id","date","time"]}}}}
            """),
    ];

    public async Task<object> ExecuteAsync(string name, JsonElement args, CancellationToken ct) => name switch
    {
        "search_medicines" => await SearchMedicinesAsync(ToolArgs.String(args, "query"), ct),
        "list_treatments" => await _catalog.ListTreatmentsAsync(ct),
        "get_free_slots" => await _catalog.GetFreeSlotsAsync(args, ct),
        "get_my_reservations" => await MyReservationsAsync(ct),
        "get_my_orders" => await MyOrdersAsync(ct),
        "suggest_actions" => await SuggestActionsAsync(args, ct),
        _ => new { error = $"Unknown tool '{name}'." }
    };

    private async Task<object> SearchMedicinesAsync(string? query, CancellationToken ct)
    {
        var words = (query ?? string.Empty)
            .Split([' ', ',', '.', ';', '/'], StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length >= 3)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(MaxSearchWords)
            .ToList();

        if (words.Count == 0)
            return NoMatch("The query had no usable keyword. Ask the patient what they are looking for.");

        // 1) Search the WHOLE active catalogue first (Lucene or SQL, whatever is configured) ...
        var found = new List<MedicineSearchItemDto>();
        foreach (var word in words)
        {
            var result = await medicineSearch.SearchAsync(word, 1, MaxSearchResults, ct);
            foreach (var item in result.Items.Where(i => found.All(f => f.Id != i.Id)))
                found.Add(item);
        }

        // 2) ... and only then limit the number of results
        found = found.Take(MaxSearchResults).ToList();
        if (found.Count == 0)
            return NoMatch("No medicine in our catalogue matches. Tell the patient we do not have it; do not recommend other medicines.");

        // availability: no inventory row = stock not tracked, treat as available
        var ids = found.Select(f => f.Id).ToList();
        var stock = await db.Inventories.AsNoTracking()
            .Where(i => ids.Contains(i.MedicineId))
            .ToDictionaryAsync(i => i.MedicineId, i => i.QuantityInStock, ct);

        return new
        {
            match = true,
            note = (string?)null,
            medicines = found.Select(m => new
            {
                id = m.Id,
                name = m.Name,
                category = m.Category,
                price = m.Price,
                description = m.Description,
                inStock = !stock.TryGetValue(m.Id, out var quantity) || quantity > 0
            })
        };
    }

    private static object NoMatch(string note) => new { match = false, note, medicines = Array.Empty<object>() };

    private async Task<object> MyReservationsAsync(CancellationToken ct)
    {
        var since = DateTime.Today.AddDays(-60);
        var rows = await db.Reservations.AsNoTracking()
            .Where(r => r.UserId == userId && r.ReservationDate >= since)
            .OrderBy(r => r.ReservationDate).ThenBy(r => r.ReservationTime)
            .Select(r => new { r.Id, r.ReservationDate, r.ReservationTime, Treatment = r.Treatment.ServiceName, Status = r.OrderStatus.StatusName, r.Price })
            .Take(30)
            .ToListAsync(ct);

        return rows.Select(r => new
        {
            id = r.Id,
            date = ToolText.Day(r.ReservationDate),
            time = ToolText.Time(r.ReservationTime),
            treatment = r.Treatment,
            status = r.Status,
            price = r.Price,
            upcoming = r.ReservationDate.Date >= DateTime.Today
        });
    }

    private async Task<object> MyOrdersAsync(CancellationToken ct)
    {
        var rows = await db.Orders.AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .Take(10)
            .Select(o => new
            {
                o.Id,
                o.OrderDate,
                Status = o.OrderStatus.StatusName,
                o.TotalPrice,
                Items = o.OrderItems.Select(i => new { i.Medicine.Name, i.Quantity })
            })
            .ToListAsync(ct);

        return rows.Select(o => new
        {
            id = o.Id,
            date = ToolText.Day(o.OrderDate),
            status = o.Status,
            total = o.TotalPrice,
            items = o.Items.Select(i => $"{i.Quantity}x {i.Name}")
        });
    }

    /// <summary>Keeps only items that really exist, are enabled and (for slots) are still free.</summary>
    private async Task<object> SuggestActionsAsync(JsonElement args, CancellationToken ct)
    {
        var medicineIds = ToolArgs.Array(args, "medicine_ids").Select(e => e.TryGetInt32(out var id) ? id : 0).Where(id => id > 0).Distinct().Take(MaxSuggestionsPerKind).ToList();
        var treatmentIds = ToolArgs.Array(args, "treatment_ids").Select(e => e.TryGetInt32(out var id) ? id : 0).Where(id => id > 0).Distinct().Take(MaxSuggestionsPerKind).ToList();
        var slotArgs = ToolArgs.Array(args, "slots").Take(MaxSuggestionsPerKind).ToList();

        var medicines = await db.Medicine.AsNoTracking()
            .Where(m => medicineIds.Contains(m.Id) && m.isEnabled)
            .Select(m => new AssistantMedicineSuggestionDto
            {
                Id = m.Id,
                Name = m.Name,
                Price = m.Price,
                InStock = !db.Inventories.Any(i => i.MedicineId == m.Id) ||
                          db.Inventories.Any(i => i.MedicineId == m.Id && i.QuantityInStock > 0)
            })
            .ToListAsync(ct);

        var slotTreatmentIds = slotArgs.Select(s => ToolArgs.Int(s, "treatment_id") ?? 0).ToList();
        var treatments = await db.Treatments.AsNoTracking()
            .Where(t => (treatmentIds.Contains(t.Id) || slotTreatmentIds.Contains(t.Id)) && t.isEnabled)
            .ToDictionaryAsync(t => t.Id, ct);

        var slots = new List<AssistantSlotSuggestionDto>();
        var rejected = new List<string>();
        foreach (var s in slotArgs)
        {
            var date = ToolArgs.Date(s, "date");
            var timeText = ToolArgs.String(s, "time");
            if (ToolArgs.Int(s, "treatment_id") is not { } tid || !treatments.TryGetValue(tid, out var treatment) ||
                date is null || !TimeSpan.TryParse(timeText, out var time) ||
                !ReservationRules.IsBookableDate(date.Value) || !ReservationRules.IsValidSlot(time))
            {
                rejected.Add($"{timeText} {ToolArgs.String(s, "date")}: invalid");
                continue;
            }

            var isTaken = await db.Reservations.AnyAsync(r =>
                r.TreatmentId == tid &&
                r.ReservationDate.Date == date.Value &&
                r.ReservationTime == time &&
                r.OrderStatus.StatusName != ReservationRules.CancelledStatus, ct);
            if (isTaken)
            {
                rejected.Add($"{ToolText.Day(date.Value)} {ToolText.Time(time)}: already taken");
                continue;
            }

            slots.Add(new AssistantSlotSuggestionDto
            {
                TreatmentId = tid,
                TreatmentName = treatment.ServiceName,
                Price = treatment.Price,
                Date = ToolText.Day(date.Value),
                Time = ToolText.Time(time)
            });
        }

        Suggestions = new AssistantSuggestionsDto
        {
            Medicines = medicines,
            // A treatment that already has a concrete slot doesn't need a second button
            Treatments = treatmentIds
                .Where(id => treatments.ContainsKey(id) && slots.All(s => s.TreatmentId != id))
                .Select(id => new AssistantTreatmentSuggestionDto { Id = id, Name = treatments[id].ServiceName, Price = treatments[id].Price })
                .ToList(),
            Slots = slots
        };

        return new
        {
            shown = new { medicines = medicines.Count, treatments = Suggestions.Treatments.Count, slots = slots.Count },
            rejected
        };
    }
}
