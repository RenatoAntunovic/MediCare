using Microsoft.AspNetCore.Mvc;
using MediCare.Infrastructure.Services;
using MediCare.Infrastructure.Models;
using MediCare.Domain.Entities.HospitalRecords;
using Microsoft.EntityFrameworkCore;
using MediCare.Infrastructure.Database;

namespace MediCare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SyncController : ControllerBase
    {
        private readonly ElasticsearchService _elasticService;
        private readonly DatabaseContext _dbContext;
        private readonly ILogger<SyncController> _logger;

        public SyncController(
            ElasticsearchService elasticService,
            DatabaseContext dbContext,
            ILogger<SyncController> logger)
        {
            _elasticService = elasticService;
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost("sync-medicines")]
        public async Task<IActionResult> SyncMedicinesToElasticsearch()
        {
            try
            {
                _logger.LogInformation("Starting medicines sync to Elasticsearch...");

                // Kreiraj Elasticsearch index
                await _elasticService.CreateProductIndexAsync();

                // Učitaj sve medicine iz SQL baze
                var medicines = await _dbContext.Medicine
                    .Include(m => m.MedicineCategory)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation($"Loaded {medicines.Count} medicines from database");

                // Mapuj Medicine → MedicineDocument
                var medicineDocuments = medicines.Select(m => new MedicineDocument
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description ?? "",
                    Price = m.Price,
                    Category = m.MedicineCategory?.Name ?? "Unknown",
                    ImagePath = m.ImagePath ?? "",
                    Weight = m.Weight,
                    IsEnabled = m.isEnabled,
                    CreatedAt = m.CreatedAtUtc
                }).ToList();

                // Bulk indexiraj u Elasticsearch
                if (medicineDocuments.Any())
                {
                    await _elasticService.IndexMedicinesBulkAsync(medicineDocuments);
                    _logger.LogInformation($"Successfully indexed {medicineDocuments.Count} medicines");
                }

                return Ok(new
                {
                    Message = $"Successfully synced {medicineDocuments.Count} medicines to Elasticsearch",
                    Count = medicineDocuments.Count,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during medicines sync");
                return StatusCode(500, new { Error = ex.Message, Details = ex.InnerException?.Message });
            }
        }
    }
}
