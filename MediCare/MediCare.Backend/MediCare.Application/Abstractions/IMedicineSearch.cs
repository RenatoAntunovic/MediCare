namespace MediCare.Application.Abstractions;

/// <summary>
/// A single medicine search result (same shape for Elasticsearch and SQL search).
/// </summary>
public sealed class MedicineSearchItemDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Category { get; init; } = string.Empty;
    public string ImagePath { get; init; } = string.Empty;
    public int Weight { get; init; }

    // The two properties below keep the response compatible with the medicine list on the client page,
    // which reads "medicineCategoryName" and "isEnabled" from every item.

    /// <summary>Same value as <see cref="Category"/>; the client medicine list uses this property name.</summary>
    public string MedicineCategoryName => Category;

    /// <summary>The index only contains enabled medicines, so this is always true.</summary>
    public bool IsEnabled => true;
}

public sealed class MedicineSearchResultDto
{
    public long Total { get; init; }
    public IReadOnlyList<MedicineSearchItemDto> Items { get; init; } = [];
}

/// <summary>
/// Medicine search. Implemented by Elasticsearch (if enabled) or the SQL fallback.
/// </summary>
public interface IMedicineSearchService
{
    Task<MedicineSearchResultDto> SearchAsync(string query, int page, int pageSize, CancellationToken ct);
}

/// <summary>
/// Search index maintenance. The index holds ONLY enabled and non-deleted medicines.
/// An implementation must never break saving to the database – errors are only logged.
/// </summary>
public interface IMedicineSearchIndex
{
    Task UpsertAsync(MedicineSearchItemDto item, CancellationToken ct);
    Task RemoveAsync(int medicineId, CancellationToken ct);
    Task RebuildAsync(IReadOnlyList<MedicineSearchItemDto> items, CancellationToken ct);
}
