using MediCare.Application.Modules.MedicineSearch;

namespace MediCare.API.Controllers;

/// <summary>
/// Manual rebuild of the search index (e.g. after Elasticsearch is started later).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SyncController(ISender sender) : ControllerBase
{
    // POST /api/sync/sync-medicines
    [HttpPost("sync-medicines")]
    public async Task<IActionResult> SyncMedicines(CancellationToken ct)
    {
        var count = await sender.Send(new RebuildMedicineSearchIndexCommand(), ct);

        return Ok(new
        {
            message = $"Index je osvježen: {count} aktivnih lijekova.",
            count,
            timestamp = DateTime.UtcNow
        });
    }
}
