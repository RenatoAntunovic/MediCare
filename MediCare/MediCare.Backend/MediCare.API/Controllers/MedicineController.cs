using MediCare.Application.Modules.Medicine.Medicine.Commands.Create;
using MediCare.Application.Modules.Medicine.Medicine.Commands.Delete;
using MediCare.Application.Modules.Medicine.Medicine.Commands.Status.Disable;
using MediCare.Application.Modules.Medicine.Medicine.Commands.Status.Enable;
using MediCare.Application.Modules.Medicine.Medicine.Commands.Update;
using MediCare.Application.Modules.Medicine.Medicine.Queries.GetById;
using MediCare.Application.Modules.Medicine.Medicine.Queries.List;

namespace MediCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicineController(ISender sender) : ControllerBase
{

    [HttpPost]
    public async Task<ActionResult<int>> CreateMedicine([FromForm] CreateMedicineCommand command, CancellationToken ct)
    {
        int id = await sender.Send(command, ct);

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task Update(int id,[FromForm] UpdateMedicineCommand command, CancellationToken ct)
    {
        // ID from the route takes precedence
        command.Id = id;
        await sender.Send(command, ct);
        // no return -> 204 No Content
    }

    [HttpDelete("{id:int}")]
    public async Task Delete(int id, CancellationToken ct)
    {
        await sender.Send(new DeleteMedicineCommand { Id = id }, ct);
        // no return -> 204 No Content
    }

    [HttpGet("{id:int}")]
    public async Task<GetMedicineByIdQueryDto> GetById(int id, CancellationToken ct)
    {
        var category = await sender.Send(new GetMedicineByIdQuery { Id = id }, ct);
        return category; // if NotFoundException -> 404 via middleware
    }

    [HttpGet]
    public async Task<PageResult<ListMedicineQueryDto>> List([FromQuery] ListMedicineQuery query, CancellationToken ct)
    {
        var result = await sender.Send(query, ct);
        return result;
    }

    [HttpPut("{id:int}/disable")]
    public async Task Disable(int id, CancellationToken ct)
    {
        await sender.Send(new DisableMedicineCommand { Id = id }, ct);
        // no return -> 204 No Content
    }

    [HttpPut("{id:int}/enable")]
    public async Task Enable(int id, CancellationToken ct)
    {
        await sender.Send(new EnableMedicineCommand { Id = id }, ct);
        // no return -> 204 No Content
    }
}