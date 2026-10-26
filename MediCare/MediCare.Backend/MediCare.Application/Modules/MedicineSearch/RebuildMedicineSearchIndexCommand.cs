namespace MediCare.Application.Modules.MedicineSearch;

/// <summary>
/// Rebuilds the complete search index from the database. Returns the number of indexed medicines.
/// </summary>
public sealed class RebuildMedicineSearchIndexCommand : IRequest<int>
{
}

public sealed class RebuildMedicineSearchIndexCommandHandler(IAppDbContext ctx, IMedicineSearchIndex index)
    : IRequestHandler<RebuildMedicineSearchIndexCommand, int>
{
    public async Task<int> Handle(RebuildMedicineSearchIndexCommand request, CancellationToken ct)
    {
        var items = await ctx.Medicine
            .AsNoTracking()
            .Where(m => m.isEnabled)
            .ToSearchItems()
            .ToListAsync(ct);

        await index.RebuildAsync(items, ct);
        return items.Count;
    }
}
