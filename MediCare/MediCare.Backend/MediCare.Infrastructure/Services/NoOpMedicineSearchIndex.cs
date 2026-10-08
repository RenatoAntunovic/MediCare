using MediCare.Application.Abstractions;

namespace MediCare.Infrastructure.Services;

/// <summary>
/// Used when Elasticsearch is NOT enabled – SQL search always reads fresh data,
/// so there is no index to maintain.
/// </summary>
public sealed class NoOpMedicineSearchIndex : IMedicineSearchIndex
{
    public Task UpsertAsync(MedicineSearchItemDto item, CancellationToken ct) => Task.CompletedTask;
    public Task RemoveAsync(int medicineId, CancellationToken ct) => Task.CompletedTask;
    public Task RebuildAsync(IReadOnlyList<MedicineSearchItemDto> items, CancellationToken ct) => Task.CompletedTask;
}
