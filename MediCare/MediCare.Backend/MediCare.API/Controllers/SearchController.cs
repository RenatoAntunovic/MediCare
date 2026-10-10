using MediCare.Application.Modules.MedicineSearch;
using Microsoft.AspNetCore.RateLimiting;

namespace MediCare.API.Controllers;

/// <summary>
/// Public full-text medicine search (Elasticsearch or SQL fallback, depending on configuration).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
[EnableRateLimiting("search")]
public class SearchController(ISender sender) : ControllerBase
{
    // GET /api/search?query=bru&page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new SearchMedicinesQuery
        {
            Query = query,
            Page = page,
            PageSize = pageSize
        }, ct);

        return Ok(new
        {
            query,
            page,
            pageSize,
            total = result.Total,
            count = result.Items.Count,
            results = result.Items
        });
    }
}
