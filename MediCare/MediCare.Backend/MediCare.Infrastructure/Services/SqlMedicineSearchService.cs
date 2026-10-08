using MediCare.Application.Abstractions;
using MediCare.Application.Modules.MedicineSearch;

namespace MediCare.Infrastructure.Services;

/// <summary>
/// SQL-based fallback search used when Elasticsearch is not enabled.
/// Every word of the query must appear in the name, description or category (AND between words).
/// Results whose name STARTS with the first word are ranked first.
/// </summary>
public sealed class SqlMedicineSearchService(IAppDbContext ctx) : IMedicineSearchService
{
    public async Task<MedicineSearchResultDto> SearchAsync(string query, int page, int pageSize, CancellationToken ct)
    {
        var terms = query
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(5)
            .ToList();

        var q = ctx.Medicine.AsNoTracking().Where(m => m.isEnabled);

        foreach (var term in terms)
        {
            var t = term; // local copy for the EF parameter
            q = q.Where(m =>
                m.Name.Contains(t) ||
                m.Description.Contains(t) ||
                m.MedicineCategory.Name.Contains(t));
        }

        var firstTerm = terms.FirstOrDefault() ?? string.Empty;

        var total = await q.LongCountAsync(ct);

        var items = await q
            .OrderByDescending(m => m.Name.StartsWith(firstTerm))
            .ThenBy(m => m.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToSearchItems()
            .ToListAsync(ct);

        return new MedicineSearchResultDto { Total = total, Items = items };
    }
}
