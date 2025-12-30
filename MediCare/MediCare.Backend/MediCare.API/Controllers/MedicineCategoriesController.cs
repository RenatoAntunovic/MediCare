using MediCare.Application.Modules.Medicine.MedicineCategories.Commands.Delete;
using MediCare.Application.Modules.Medicine.MedicineCategories.Commands.Status.Disable;
using MediCare.Application.Modules.Medicine.MedicineCategories.Commands.Status.Enable;
using MediCare.Application.Modules.Medicine.MedicineCategories.Commands.Create;
using MediCare.Application.Modules.Medicine.MedicineCategories.Commands.Update;
using MediCare.Application.Modules.Medicine.MedicineCategories.Queries.GetById;
using MediCare.Application.Modules.Medicine.MedicineCategories.Queries.List;
using MediCare.API.FCM;

namespace MediCare.API.Controllers;

[ApiController]
[Route("[controller]")]
public class MedicineCategoriesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IFcmService _fcmService;

    public MedicineCategoriesController(ISender sender, IFcmService fcmService)
    {
        _sender = sender;
        _fcmService = fcmService;
    }
    [HttpPost]
    public async Task<ActionResult<int>> CreateMedicineCategory(CreateMedicineCategoryCommand command, CancellationToken ct)
    {
        int id = await _sender.Send(command, ct);

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task Update(int id, UpdateMedicineCategoryCommand command, CancellationToken ct)
    {
        // ID from the route takes precedence
        command.Id = id;
        await _sender.Send(command, ct);
        // no return -> 204 No Content
    }

    [HttpDelete("{id:int}")]
    public async Task Delete(int id, CancellationToken ct)
    {
        await _sender.Send(new DeleteMedicineCategoryCommand { Id = id }, ct);
        // no return -> 204 No Content
    }

    [HttpGet("{id:int}")]
    public async Task<GetMedicineCategoryByIdQueryDto> GetById(int id, CancellationToken ct)
    {
        var category = await _sender.Send(new GetMedicineCategoryByIdQuery { Id = id }, ct);
        return category; // if NotFoundException -> 404 via middleware
    }

    [HttpGet]
    public async Task<PageResult<ListMedicineCategoriesQueryDto>> List([FromQuery] ListMedicineCategoriesQuery query, CancellationToken ct)
    {
        var result = await _sender.Send(query, ct);
        return result;
    }

    [HttpPut("{id:int}/disable")]
    public async Task Disable(int id, CancellationToken ct)
    {
        await _sender.Send(new DisableMedicineCategoryCommand { Id = id }, ct);
        // no return -> 204 No Content
    }

    [HttpPut("{id:int}/enable")]
    public async Task Enable(int id, CancellationToken ct)
    {
        await _sender.Send(new EnableMedicineCategoryCommand { Id = id }, ct);
        // no return -> 204 No Content
    } 

}