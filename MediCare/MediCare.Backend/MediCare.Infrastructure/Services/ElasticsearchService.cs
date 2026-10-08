using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using MediCare.Application.Abstractions;
using MediCare.Infrastructure.Models;
using Microsoft.Extensions.Logging;

namespace MediCare.Infrastructure.Services
{
    /// <summary>
    /// Elasticsearch implementation of search and index maintenance.
    /// The index contains only active medicines, so public search cannot return a disabled medicine.
    /// </summary>
    public class ElasticsearchService : IMedicineSearchService, IMedicineSearchIndex
    {
        private readonly ElasticsearchClient _client;
        private readonly ILogger<ElasticsearchService> _logger;
        private const string ProductIndexName = "products";

        public ElasticsearchService(ElasticsearchClient client, ILogger<ElasticsearchService> logger)
        {
            _client = client;
            _logger = logger;
        }

        // =========================================================
        // SEARCH
        // =========================================================

        public async Task<MedicineSearchResultDto> SearchAsync(string query, int page, int pageSize, CancellationToken ct)
        {
            var response = await _client.SearchAsync<MedicineDocument>(s => s
                .Indices(ProductIndexName)
                .From((page - 1) * pageSize)
                .Size(pageSize)
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Fields(new[] { "name", "description", "category" })
                        .Query(query)
                        .Type(TextQueryType.BoolPrefix)
                        .Fuzziness(new Fuzziness("AUTO"))
                        .Operator(Operator.Or)
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                _logger.LogError("Elasticsearch search failed: {Debug}", response.DebugInformation);
                throw new InvalidOperationException("Pretraga trenutno nije dostupna.");
            }

            return new MedicineSearchResultDto
            {
                Total = response.Total,
                Items = response.Documents.Select(ToDto).ToList()
            };
        }

        // =========================================================
        // INDEX MAINTENANCE
        // =========================================================

        public async Task UpsertAsync(MedicineSearchItemDto item, CancellationToken ct)
        {
            try
            {
                var response = await _client.IndexAsync(ToDocument(item), ProductIndexName);
                if (!response.IsValidResponse)
                    _logger.LogWarning("Elasticsearch upsert za lijek {Id} nije uspio: {Debug}", item.Id, response.DebugInformation);
            }
            catch (Exception ex)
            {
                // The index must never break saving to the database.
                _logger.LogWarning(ex, "Elasticsearch upsert za lijek {Id} nije uspio.", item.Id);
            }
        }

        public async Task RemoveAsync(int medicineId, CancellationToken ct)
        {
            try
            {
                // 404 (document does not exist) is not an error – just ignore it.
                await _client.DeleteAsync<MedicineDocument>(medicineId, d => d.Index(ProductIndexName));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Elasticsearch brisanje lijeka {Id} nije uspjelo.", medicineId);
            }
        }

        public async Task RebuildAsync(IReadOnlyList<MedicineSearchItemDto> items, CancellationToken ct)
        {
            await DeleteProductIndexAsync();
            await CreateProductIndexAsync();

            if (items.Count == 0)
                return;

            var documents = items.Select(ToDocument).ToList();

            var bulkResponse = await _client.BulkAsync(b => b
                .Index(ProductIndexName)
                .IndexMany(documents)
            );

            if (!bulkResponse.IsValidResponse)
                throw new InvalidOperationException($"Bulk index failed: {bulkResponse.DebugInformation}");
        }

        // =========================================================
        // INDEX (create / delete)
        // =========================================================

        public async Task CreateProductIndexAsync()
        {
            var existsResponse = await _client.Indices.ExistsAsync(ProductIndexName);

            if (!existsResponse.Exists)
            {
                await _client.Indices.CreateAsync(ProductIndexName, c => c
                    .Mappings(m => m
                        .Properties<MedicineDocument>(p => p
                            .IntegerNumber(n => n.Id)
                            .Text(t => t.Name, td => td.Analyzer("standard"))
                            .Text(t => t.Description, td => td.Analyzer("standard"))
                            .Text(t => t.Category, td => td.Analyzer("standard"))
                            .DoubleNumber(n => n.Price)
                            .Text(t => t.ImagePath)
                            .IntegerNumber(n => n.Weight)
                            .Boolean(b => b.IsEnabled)
                            .Date(d => d.CreatedAt)
                        )
                    )
                );
            }
        }

        public async Task DeleteProductIndexAsync()
        {
            var existsResponse = await _client.Indices.ExistsAsync(ProductIndexName);

            if (existsResponse.Exists)
            {
                await _client.Indices.DeleteAsync(ProductIndexName);
            }
        }

        // =========================================================
        // MAPPING
        // =========================================================

        private static MedicineDocument ToDocument(MedicineSearchItemDto item) => new()
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            Category = item.Category,
            ImagePath = item.ImagePath,
            Weight = item.Weight,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };

        private static MedicineSearchItemDto ToDto(MedicineDocument d) => new()
        {
            Id = d.Id,
            Name = d.Name ?? string.Empty,
            Description = d.Description ?? string.Empty,
            Price = d.Price,
            Category = d.Category ?? string.Empty,
            ImagePath = d.ImagePath ?? string.Empty,
            Weight = d.Weight
        };
    }
}
