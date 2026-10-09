using MediatR;
using MediCare.API.FCM;
using MediCare.API.Reports;
using MediCare.Application.Modules.FCM;
using MediCare.Application.Modules.Sales.Orders.Commands.Create;
using MediCare.Application.Modules.Sales.Orders.Commands.Status;
using MediCare.Application.Modules.Sales.Orders.Commands.Update;
using MediCare.Application.Modules.Sales.Orders.Queries.GetById;
using MediCare.Application.Modules.Sales.Orders.Queries.List;
using MediCare.Application.Modules.Sales.Orders.Queries.ListWithItems;
using MediCare.Application.Modules.Sales.Orders.Queries.Report;


namespace Market.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
    public async Task Update(int id, UpdateOrderCommand command, CancellationToken ct)
    {
        command.Id = id;
        await _sender.Send(command, ct);
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
    [Authorize(Roles = "Admin")]
    public async Task ChangeStatus(int id, [FromBody] ChangeOrderStatusCommand command, CancellationToken ct)
    {
        command.Id = id;
        await _sender.Send(command, ct);
    }

    // =========================================================
    // PDF – single order
    // GET /Orders/5/pdf
    // Admin sees all orders, a user only their own (checked in GetOrderByIdQueryHandler).
    // =========================================================
    [HttpGet("{id:int}/pdf")]
    [Authorize]
    public async Task<IActionResult> GeneratePdf(int id, CancellationToken ct)
    {
        var order = await _sender.Send(new GetOrderByIdQuery { Id = id }, ct);
        var bytes = OrderPdfBuilder.BuildOrderPdf(order);

        return File(bytes, "application/pdf", $"Narudzba_{id}_{DateTime.Now:yyyyMMdd}.pdf");
    }

    // =========================================================
    // PDF – parameterized report
    // GET /Orders/report/pdf?from=2026-01-01&to=2026-01-31&statusId=4
    // =========================================================
    [HttpGet("report/pdf")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GenerateReportPdf([FromQuery] OrdersReportQuery query, CancellationToken ct)
    {
        var report = await _sender.Send(query, ct);
        var bytes = OrderPdfBuilder.BuildOrdersReportPdf(report);

        return File(bytes, "application/pdf", $"Izvjestaj_narudzbi_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }
}
