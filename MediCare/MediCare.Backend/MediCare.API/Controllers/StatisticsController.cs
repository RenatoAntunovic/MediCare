using MediCare.Application.Modules.Statistics.Queries.Dashboard;

namespace MediCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class StatisticsController(ISender sender) : ControllerBase
{
    // GET /api/Statistics/dashboard?from=2026-01-01&to=2026-06-30
    [HttpGet("dashboard")]
    public async Task<StatisticsDashboardDto> Dashboard([FromQuery] StatisticsDashboardQuery query, CancellationToken ct)
    {
        return await sender.Send(query, ct);
    }
}
