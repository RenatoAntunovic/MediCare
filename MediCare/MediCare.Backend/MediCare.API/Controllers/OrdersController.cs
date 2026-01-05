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
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;


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

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> GeneratePdf(int id, CancellationToken ct)
    {
        Console.WriteLine($"=== PDF REQUEST for Order ID: {id} ===");

        try
        {
            // Preuzmi podatke o narudžbi
            var order = await _sender.Send(new GetOrderByIdQuery { Id = id }, ct);

            Console.WriteLine($"Order found: {order != null}");

            if (order == null)
            {
                Console.WriteLine("Order is NULL - returning 404");
                return NotFound();
            }

            Console.WriteLine($"Order has {order.Items?.Count ?? 0} items");

            // Kreiraj PDF u memoriji
            using var memoryStream = new MemoryStream();

            // Kreiraj dokument
            var document = new Document(PageSize.A4, 50, 50, 25, 25);
            var writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            // Font
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            // Naslov
            var title = new Paragraph("IZVJEŠTAJ O NARUDŽBI", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 20;
            document.Add(title);

            // Informacije o narudžbi
            document.Add(new Paragraph($"ID Narudžbe: {order.Id}", headerFont));
            document.Add(new Paragraph($"Datum narudžbe: {order.OrderDate:dd.MM.yyyy HH:mm}", normalFont));
            document.Add(new Paragraph($"Status: {order.StatusName}", normalFont));
            document.Add(new Paragraph(" "));

            // Informacije o kupcu
            document.Add(new Paragraph("INFORMACIJE O KUPCU", headerFont));
            document.Add(new Paragraph($"Ime: {order.User.UserFirstname} {order.User.UserLastname}", normalFont));
            document.Add(new Paragraph($"Adresa: {order.User.UserAddress}", normalFont));
            document.Add(new Paragraph($"Grad: {order.User.UserCity}", normalFont));
            document.Add(new Paragraph(" "));

            // Tabela sa stavkama narudžbe
            document.Add(new Paragraph("STAVKE NARUDŽBE", headerFont));
            document.Add(new Paragraph(" "));

            var table = new PdfPTable(4);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 3, 1, 2, 2 });

            // Header tabele
            AddTableHeader(table, "Lijek");
            AddTableHeader(table, "Količina");
            AddTableHeader(table, "Cijena");
            AddTableHeader(table, "Ukupno");

            // Dodaj stavke i računaj ukupnu cijenu
            decimal totalPrice = 0;
            foreach (var item in order.Items)
            {
                decimal itemTotal = item.Price * item.Quantity;
                totalPrice += itemTotal;

                table.AddCell(new PdfPCell(new Phrase(item.Medicine.MedicineName, normalFont)));
                table.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString(), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase($"{item.Price:F2} KM", normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                table.AddCell(new PdfPCell(new Phrase($"{itemTotal:F2} KM", normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            document.Add(table);
            document.Add(new Paragraph(" "));

            // Ukupna cijena
            var total = new Paragraph($"UKUPNO: {totalPrice:F2} KM", headerFont);
            total.Alignment = Element.ALIGN_RIGHT;
            document.Add(total);

            // Datum generisanja
            document.Add(new Paragraph(" "));
            var generatedDate = new Paragraph($"Izvještaj generisan: {DateTime.Now:dd.MM.yyyy HH:mm}", normalFont);
            generatedDate.Alignment = Element.ALIGN_LEFT;
            generatedDate.SpacingBefore = 20;
            document.Add(generatedDate);

            document.Close();

            Console.WriteLine("PDF created successfully");

            // Vrati PDF kao file download
            var bytes = memoryStream.ToArray();
            return File(bytes, "application/pdf", $"Narudzba_{id}_{DateTime.Now:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine($"STACK: {ex.StackTrace}");
            return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
        }
    }


    // Helper metoda za header tabele
    private void AddTableHeader(PdfPTable table, string text)
    {
        var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.White);
        var cell = new PdfPCell(new Phrase(text, headerFont));
        cell.BackgroundColor = new BaseColor(52, 73, 94); // Tamno plava
        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        cell.Padding = 5;
        table.AddCell(cell);
    }


}