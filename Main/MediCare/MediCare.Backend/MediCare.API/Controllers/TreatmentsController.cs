using MediCare.Application.Modules.Catalog.Treatments.Commands.Delete;
using MediCare.Application.Modules.Catalog.Treatments.Commands.Status.Disable;
using MediCare.Application.Modules.Catalog.Treatments.Commands.Status.Enable;
using MediCare.Application.Modules.Catalog.Treatments.Commands.Create;
using MediCare.Application.Modules.Catalog.Treatments.Commands.Update;
using MediCare.Application.Modules.Catalog.Treatments.Queries.GetById;
using MediCare.Application.Modules.Catalog.Treatments.Queries.List;

namespace MediCare.API.Controllers;

[ApiController]
[Route("[controller]")]
public class TreatmentsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> CreateTreatmentsCategory(CreateTreatmentsCommand command, CancellationToken ct)
    {
        int id = await sender.Send(command, ct);

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task Update(int id, UpdateTreatmentsCommand command, CancellationToken ct)
    {
        // ID from the route takes precedence
        command.Id = id;
        await sender.Send(command, ct);
        // no return -> 204 No Content
    }

    [HttpDelete("{id:int}")]
    public async Task Delete(int id, CancellationToken ct)
    {
        await sender.Send(new DeleteTreatmentsCommand { Id = id }, ct);
        // no return -> 204 No Content
    }

    [HttpGet("{id:int}")]
    public async Task<GetTreatmentsByIdQueryDto> GetById(int id, CancellationToken ct)
    {
        var treatments = await sender.Send(new GetTreatmentsByIdQuery { Id = id }, ct);
        return treatments; // if NotFoundException -> 404 via middleware
    }

    [HttpGet]
    public async Task<PageResult<ListTreatmentsQueryDto>> List([FromQuery] ListTreatmentsQuery query, CancellationToken ct)
    {
        var result = await sender.Send(query, ct);
        return result;
    }

    [HttpPut("{id:int}/disable")]
    public async Task Disable(int id, CancellationToken ct)
    {
        await sender.Send(new DisableTreatmentsCommand { Id = id }, ct);
        // no return -> 204 No Content
    }

    [HttpPut("{id:int}/enable")]
    public async Task Enable(int id, CancellationToken ct)
    {
        await sender.Send(new EnableTreatmentsCommand { Id = id }, ct);
        // no return -> 204 No Content
    }
}