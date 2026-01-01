using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using MediCare.Application.Abstractions;
using MediCare.Domain.Entities;
using MediCare.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Infrastructure.Services
{
    public class ElasticsearchService
    {
        private readonly ElasticsearchClient _client;
        private const string ProductIndexName = "products";
        private const string MedicineIndexName = "medicines";

        public ElasticsearchService(ElasticsearchClient client)
        {
            _client = client;
        }

        // Kreiraj index
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

        // Indexiraj proizvod
        public async Task IndexMedicineAsync(MedicineDocument medicine)
        {
            await _client.IndexAsync(medicine, ProductIndexName);
        }

        // Bulk indexiranje
        public async Task IndexMedicinesBulkAsync(List<MedicineDocument> medicines)
        {
            var bulkResponse = await _client.BulkAsync(b => b
                .Index(ProductIndexName)
                .IndexMany(medicines)
            );

            if (!bulkResponse.IsValidResponse)
            {
                throw new Exception($"Bulk index failed: {bulkResponse.DebugInformation}");
            }
        }

        // Full-Text Search
        public async Task<List<MedicineDocument>> SearchMedicinesAsync(string query, int page = 1, int pageSize = 10)
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

            return response.Documents.ToList();
        }

        // Obriši proizvod
        public async Task DeleteMedicineAsync(int medicineId)
        {
            await _client.DeleteAsync<MedicineDocument>(medicineId, d => d.Index(ProductIndexName));
        }

        // Update proizvod
        public async Task UpdateMedicineAsync(MedicineDocument medicine)
        {
            await _client.UpdateAsync<MedicineDocument, MedicineDocument>(
                ProductIndexName,
                medicine.Id,
                u => u.Doc(medicine)
            );
        }

        // Obriši index
        public async Task DeleteProductIndexAsync()
        {
            var existsResponse = await _client.Indices.ExistsAsync(ProductIndexName);

            if (existsResponse.Exists)
            {
                await _client.Indices.DeleteAsync(ProductIndexName);
            }
        }

        // Sinhroniziraj ljekove iz baze u Elasticsearch
        public async Task SyncMedicinesFromDatabaseAsync(IAppDbContext dbContext)
        {
            // Učitaj sve ljekove iz baze
            var medicines = await dbContext.Medicine
                .Include(m => m.MedicineCategory)
                .AsNoTracking()
                .ToListAsync();

            if (!medicines.Any())
            {
                return;
            }

            // Mapiraj u MedicineDocument
            var documents = medicines.Select(m => new MedicineDocument
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                Category = m.MedicineCategory.Name,
                ImagePath = m.ImagePath,
                Weight = m.Weight,
                IsEnabled = m.isEnabled,
                CreatedAt = m.CreatedAtUtc
            }).ToList();

            // Bulk index u Elasticsearch
            await IndexMedicinesBulkAsync(documents);
        }
    }
}
