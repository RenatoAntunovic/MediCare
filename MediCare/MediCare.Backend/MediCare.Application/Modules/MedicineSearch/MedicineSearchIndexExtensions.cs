using MedicineEntity = MediCare.Domain.Entities.HospitalRecords.Medicine;

namespace MediCare.Application.Modules.MedicineSearch;

public static class MedicineSearchIndexExtensions
{
    /// <summary>
    /// Projection of the entity to the search DTO (used for both the index and SQL search).
    /// </summary>
    public static IQueryable<MedicineSearchItemDto> ToSearchItems(this IQueryable<MedicineEntity> query) =>
        query.Select(m => new MedicineSearchItemDto
        {
            Id = m.Id,
            Name = m.Name,
            Description = m.Description ?? string.Empty,
            Price = m.Price,
            Category = m.MedicineCategory.Name,
            ImagePath = m.ImagePath ?? string.Empty,
            Weight = m.Weight
        });

    /// <summary>
    /// Synchronizes one medicine in the index with its state in the database:
    /// active → upsert; disabled/deleted/missing → remove from the index.
    /// Call AFTER SaveChangesAsync.
    /// </summary>
    public static async Task SyncMedicineAsync(
        this IMedicineSearchIndex index,
        IAppDbContext ctx,
        int medicineId,
        CancellationToken ct)
    {
        var item = await ctx.Medicine
            .AsNoTracking()
            .Where(m => m.Id == medicineId && m.isEnabled)
            .ToSearchItems()
            .FirstOrDefaultAsync(ct);

        if (item is null)
            await index.RemoveAsync(medicineId, ct);
        else
            await index.UpsertAsync(item, ct);
    }
}
