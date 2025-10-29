using MediCare.Application.Modules.Catalog.ProductCategories.Commands.Delete;
using MediCare.Application.Modules.Catalog.ProductCategories.Commands.Status.Disable;
using MediCare.Application.Modules.Catalog.ProductCategories.Commands.Status.Enable;
using MediCare.Application.Modules.Catalog.ProductCategories.Commands.Create;
using MediCare.Application.Modules.Catalog.ProductCategories.Commands.Update;
using MediCare.Application.Modules.Catalog.ProductCategories.Queries.GetById;
using MediCare.Application.Modules.Catalog.ProductCategories.Queries.List;

namespace MediCare.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductCategoriesController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> CreateProductCategory(CreateMedicineCategoryCommand command, CancellationToken ct)
    {
        int id = await sender.Send(command, ct);

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task Update(int id, UpdateMedicineCategoryCommand command, CancellationToken ct)
    {
        // ID from the route takes precedence
        command.Id = id;
        await sender.Send(command, ct);
        // no return -> 204 No Content
    }

    [HttpDelete("{id:int}")]
    public async Task Delete(int id, CancellationToken ct)
    {
        await sender.Send(new DeleteMedicineCategoryCommand { Id = id }, ct);
        // no return -> 204 No Content
    }

    [HttpGet("{id:int}")]
    public async Task<GetMedicineCategoryByIdQueryDto> GetById(int id, CancellationToken ct)
    {
        var category = await sender.Send(new GetMedicineCategoryByIdQuery { Id = id }, ct);
        return category; // if NotFoundException -> 404 via middleware
    }

    [HttpGet]
    public async Task<PageResult<ListMedicineCategoriesQueryDto>> List([FromQuery] ListMedicineCategoriesQuery query, CancellationToken ct)
    {
        var result = await sender.Send(query, ct);
        return result;
    }

    [HttpPut("{id:int}/disable")]
    public async Task Disable(int id, CancellationToken ct)
    {
        await sender.Send(new DisableMedicineCategoryCommand { Id = id }, ct);
        // no return -> 204 No Content
    }

    [HttpPut("{id:int}/enable")]
    public async Task Enable(int id, CancellationToken ct)
    {
        await sender.Send(new EnableMedicineCategoryCommand { Id = id }, ct);
        // no return -> 204 No Content
    }
}