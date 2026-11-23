using MediCare.Application.Modules.Procurement.Suppliers.Commands.Create;
using MediCare.Application.Modules.Procurement.Suppliers.Commands.Delete;
using MediCare.Application.Modules.Procurement.Suppliers.Commands.Status;
using MediCare.Application.Modules.Procurement.Suppliers.Commands.Update;
using MediCare.Application.Modules.Procurement.Suppliers.Queries.GetById;
using MediCare.Application.Modules.Procurement.Suppliers.Queries.List;

namespace MediCare.API.Controllers;

/// <summary>Supplier management (admin only).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SuppliersController(ISender sender) : ControllerBase
{
    // GET /api/suppliers?search=&city=&onlyEnabled=&medicineId=&createdFrom=&paging.page=1&paging.pageSize=10
    [HttpGet]
    public Task<PageResult<ListSuppliersQueryDto>> List([FromQuery] ListSuppliersQuery query, CancellationToken ct)
        => sender.Send(query, ct);

    [HttpGet("cities")]
    public Task<List<string>> Cities(CancellationToken ct)
        => sender.Send(new ListSupplierCitiesQuery(), ct);

    [HttpGet("{id:int}")]
    public Task<GetSupplierByIdQueryDto> GetById(int id, CancellationToken ct)
        => sender.Send(new GetSupplierByIdQuery { Id = id }, ct);

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateSupplierCommand command, CancellationToken ct)
    {
        var id = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task Update(int id, UpdateSupplierCommand command, CancellationToken ct)
    {
        command.Id = id; // the id from the route takes precedence
        await sender.Send(command, ct);
    }

    [HttpDelete("{id:int}")]
    public async Task Delete(int id, CancellationToken ct)
        => await sender.Send(new DeleteSupplierCommand { Id = id }, ct);

    [HttpPut("{id:int}/enable")]
    public async Task Enable(int id, CancellationToken ct)
        => await sender.Send(new EnableSupplierCommand { Id = id }, ct);

    [HttpPut("{id:int}/disable")]
    public async Task Disable(int id, CancellationToken ct)
        => await sender.Send(new DisableSupplierCommand { Id = id }, ct);
}
