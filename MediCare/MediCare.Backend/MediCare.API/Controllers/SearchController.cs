using Microsoft.AspNetCore.Mvc;
using MediCare.Infrastructure.Services;
using MediCare.Infrastructure.Models;

namespace MediCare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ElasticsearchService _elasticService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ElasticsearchService elasticService, ILogger<SearchController> logger)
        {
            _elasticService = elasticService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] string query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return BadRequest(new { Message = "Query is required" });

                _logger.LogInformation($"Searching for: {query}");

                var results = await _elasticService.SearchMedicinesAsync(query, page, pageSize);

                return Ok(new
                {
                    Query = query,
                    Page = page,
                    PageSize = pageSize,
                    Results = results,
                    Count = results.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search error");
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
