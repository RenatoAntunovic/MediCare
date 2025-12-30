using MediatR;
using MediCare.API.FCM;
using MediCare.Application.Modules.Cart.Command.Checkout;
using MediCare.Application.Modules.FCM;
using MediCare.Application.Modules.FCM.Services;
using MediCare.Application.Modules.Sales.Orders.Commands.Create;
using MediCare.Application.Modules.Sales.Orders.Commands.Status;
using MediCare.Application.Modules.Sales.Orders.Commands.Update;
using MediCare.Application.Modules.Sales.Orders.Queries.GetById;
using MediCare.Application.Modules.Sales.Orders.Queries.List;
using MediCare.Application.Modules.Sales.Orders.Queries.ListWithItems;

namespace Market.API.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IFcmService _fcmService;

    public OrdersController(ISender sender, IFcmService fcmService)
    {
        _sender = sender;
        _fcmService = fcmService;
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateOrderCommand command, CancellationToken ct)
    {
        int id = await _sender.Send(command, ct);

        if (SaveFcmTokenHandler.TryGetToken(command.UserId, out var fcmToken))
        {
            await _fcmService.SendNotificationAsync(
                fcmToken,
                "Nova narudžba",
                $"Imate novu narudžbu #{id}"
            );
        }

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task Update(int id, UpdateOrderCommand command, CancellationToken ct)
    {
        // ID from the route takes precedence
        command.Id = id;
        await _sender.Send(command, ct);
        // no return -> 204 No Content
    }

    [HttpGet("{id:int}")]
    public async Task<GetOrderByIdQueryDto> GetById(int id, CancellationToken ct)
    {
        var dto = await _sender.Send(new GetOrderByIdQuery { Id = id }, ct);
        return dto; // if NotFoundException -> 404 via middleware
    }
    [HttpGet]
    public async Task<PageResult<ListOrdersQueryDto>> List([FromQuery] ListOrdersQuery query, CancellationToken ct)
    {
        var result = await _sender.Send(query, ct);
        return result;
    }

    [HttpGet("with-items")]
    public async Task<PageResult<ListOrdersWithItemsQueryDto>> ListWithItems([FromQuery] ListOrdersWithItemsQuery query, CancellationToken ct)
    {
        var result = await _sender.Send(query, ct);
        return result;
    }

    [HttpPut("{id:int}/change-status")]
    public async Task ChangeStatus(int id, [FromBody] ChangeOrderStatusCommand command, CancellationToken ct)
    {
        command.Id = id;
        await _sender.Send(command, ct);
        // no return -> 204 No Content
    }

}