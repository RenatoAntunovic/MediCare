using MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Delete;
using MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Status.Disable;
using MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Status.Enable;
using MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Create;
using MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Update;
using MediCare.Application.Modules.Catalog.TreatmentCategories.Queries.GetById;
using MediCare.Application.Modules.Catalog.TreatmentCategories.Queries.List;

namespace MediCare.API.Controllers;

[ApiController]
[Route("[controller]")]
public class TreatmentCategoriesController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> CreateTreatmentCategory(CreateTreatmentCategoryCommand command, CancellationToken ct)
    {
        int id = await sender.Send(command, ct);

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task Update(int id, UpdateTreatmentCategoryCommand command, CancellationToken ct)
    {
        // ID from the route takes precedence
        command.Id = id;
        await sender.Send(command, ct);
        // no return -> 204 No Content
    }

    [HttpDelete("{id:int}")]
    public async Task Delete(int id, CancellationToken ct)
    {
        await sender.Send(new DeleteTreatmentCategoryCommand { Id = id }, ct);
        // no return -> 204 No Content
    }

    [HttpGet("{id:int}")]
    public async Task<GetTreatmentCategoryByIdQueryDto> GetById(int id, CancellationToken ct)
    {
        var category = await sender.Send(new GetTreatmentCategoryByIdQuery { Id = id }, ct);
        return category; // if NotFoundException -> 404 via middleware
    }

    [HttpGet]
    public async Task<PageResult<ListTreatmentCategoriesQueryDto>> List([FromQuery] ListTreatmentCategoriesQuery query, CancellationToken ct)
    {
        var result = await sender.Send(query, ct);
        return result;
    }

    [HttpPut("{id:int}/disable")]
    public async Task Disable(int id, CancellationToken ct)
    {
        await sender.Send(new DisableTreatmentCategoryCommand { Id = id }, ct);
        // no return -> 204 No Content
    }

    [HttpPut("{id:int}/enable")]
    public async Task Enable(int id, CancellationToken ct)
    {
        await sender.Send(new EnableTreatmentCategoryCommand { Id = id }, ct);
        // no return -> 204 No Content
    }
}