using iTextSharp.text.pdf;
using MediCare.Application.Modules.Sales.Orders.Queries.Report;
using iText = iTextSharp.text;

namespace MediCare.API.Reports;

/// <summary>
/// Generates PDF documents for orders.
/// Uses a TTF font with Unicode support (č, ć, đ, š, ž). Font lookup order:
///   1) MediCare.API/wwwroot/fonts/DejaVuSans.ttf (recommended – works on any machine)
///   2) Arial from Windows / macOS, or DejaVu on Linux
///   3) Helvetica (č/ć/đ will not be rendered in that case)
/// </summary>
public static class OrderPdfBuilder
{
    // Element.ALIGN_* values from iTextSharp (as constants so they can be used as default parameters)
    private const int AlignLeft = 0;    // Element.ALIGN_LEFT
    private const int AlignCenter = 1;  // Element.ALIGN_CENTER
    private const int AlignRight = 2;   // Element.ALIGN_RIGHT

    private static readonly iText.BaseColor Black = new(0, 0, 0);
    private static readonly iText.BaseColor White = new(255, 255, 255);
    private static readonly iText.BaseColor Brand = new(73, 118, 181);
    private static readonly iText.BaseColor HeaderBackground = new(52, 73, 94);
    private static readonly iText.BaseColor ZebraBackground = new(245, 247, 250);

    private static readonly Lazy<BaseFont?> UnicodeFont = new(LoadUnicodeFont);

    private static readonly Dictionary<string, string> StatusLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["DRAFT"] = "Nacrt",
        ["CONFIRMED"] = "Potvrđena",
        ["PAID"] = "Plaćena",
        ["COMPLETED"] = "Završena",
        ["CANCELLED"] = "Otkazana"
    };

    // =========================================================
    // 1) SINGLE ORDER PDF
    // =========================================================
    public static byte[] BuildOrderPdf(GetOrderByIdQueryDto order)
    {
        using var ms = new MemoryStream();
        var document = new iText.Document(iText.PageSize.A4, 50, 50, 30, 30);
        PdfWriter.GetInstance(document, ms);
        document.Open();

        AddTitle(document, "Izvještaj o narudžbi");

        document.Add(Text($"Narudžba #{order.Id}", Font(12, bold: true)));
        document.Add(Text($"Datum narudžbe: {order.OrderDate:dd.MM.yyyy HH:mm}", Font(10)));
        document.Add(Text($"Status: {StatusLabel(order.StatusName)}", Font(10)));
        document.Add(Spacer());

        document.Add(Text("Kupac", Font(12, bold: true)));
        document.Add(Text($"{order.User.UserFirstname} {order.User.UserLastname}", Font(10)));
        document.Add(Text($"{order.User.UserAddress}, {order.User.UserCity}", Font(10)));
        document.Add(Spacer());

        var table = CreateTable(new float[] { 4, 1.2f, 2, 2 });
        AddHeader(table, "Lijek", "Količina", "Cijena/kom", "Ukupno");

        decimal orderTotal = 0;
        var rowIndex = 0;

        foreach (var item in order.Items)
        {
            // IMPORTANT: OrderItems.Price is the TOTAL for the line (Medicine.Price * Quantity),
            // so it must NOT be multiplied by the quantity again here.
            var lineTotal = item.Price;
            var unitPrice = item.Quantity > 0 ? Math.Round(item.Price / item.Quantity, 2) : item.Price;
            orderTotal += lineTotal;

            var zebra = rowIndex++ % 2 == 1;
            AddCell(table, item.Medicine.MedicineName, zebra);
            AddCell(table, item.Quantity.ToString(), zebra, AlignCenter);
            AddCell(table, Money(unitPrice), zebra, AlignRight);
            AddCell(table, Money(lineTotal), zebra, AlignRight);
        }

        document.Add(table);
        document.Add(Spacer());
        document.Add(Text($"UKUPNO: {Money(orderTotal)}", Font(13, bold: true), AlignRight));

        AddFooter(document);
        document.Close();
        return ms.ToArray();
    }

    // =========================================================
    // 2) PARAMETERIZED REPORT (period + status)
    // =========================================================
    public static byte[] BuildOrdersReportPdf(OrdersReportDto report)
    {
        using var ms = new MemoryStream();
        var document = new iText.Document(iText.PageSize.A4, 40, 40, 30, 30);
        PdfWriter.GetInstance(document, ms);
        document.Open();

        AddTitle(document, "Izvještaj o narudžbama");

        // --- Parameters ---
        document.Add(Text("Parametri izvještaja", Font(12, bold: true)));
        document.Add(Text($"Period: {PeriodText(report.From, report.To)}", Font(10)));
        document.Add(Text($"Status: {(report.StatusName is null ? "Svi statusi" : StatusLabel(report.StatusName))}", Font(10)));
        document.Add(Spacer());

        // --- Summary ---
        document.Add(Text("Sažetak", Font(12, bold: true)));
        var summary = CreateTable(new float[] { 3, 2 });
        summary.WidthPercentage = 55;
        summary.HorizontalAlignment = AlignLeft;
        AddCell(summary, "Broj narudžbi", false);
        AddCell(summary, report.OrdersCount.ToString(), false, AlignRight);
        AddCell(summary, "Prodanih komada", true);
        AddCell(summary, report.ItemsCount.ToString(), true, AlignRight);
        AddCell(summary, "Ukupan promet", false, bold: true);
        AddCell(summary, Money(report.TotalRevenue), false, AlignRight, bold: true);
        document.Add(summary);
        document.Add(Spacer());

        if (report.Rows.Count == 0)
        {
            document.Add(Text("Nema narudžbi za odabrane parametre.", Font(11)));
            AddFooter(document);
            document.Close();
            return ms.ToArray();
        }

        // --- By status (only when not filtered to a single status) ---
        if (report.StatusName is null && report.ByStatus.Count > 1)
        {
            document.Add(Text("Po statusu", Font(12, bold: true)));
            var statusTable = CreateTable(new float[] { 3, 2, 2 });
            statusTable.WidthPercentage = 70;
            statusTable.HorizontalAlignment = AlignLeft;
            AddHeader(statusTable, "Status", "Narudžbi", "Iznos");

            var i = 0;
            foreach (var s in report.ByStatus)
            {
                var zebra = i++ % 2 == 1;
                AddCell(statusTable, StatusLabel(s.StatusName), zebra);
                AddCell(statusTable, s.OrdersCount.ToString(), zebra, AlignCenter);
                AddCell(statusTable, Money(s.Total), zebra, AlignRight);
            }

            document.Add(statusTable);
            document.Add(Spacer());
        }

        // --- Order list ---
        document.Add(Text("Narudžbe", Font(12, bold: true)));
        var table = CreateTable(new float[] { 1, 2.2f, 3.2f, 2, 1.3f, 2 });
        table.HeaderRows = 1; // header row repeats on every page
        AddHeader(table, "#", "Datum", "Kupac", "Status", "Kom.", "Iznos");

        var row = 0;
        foreach (var r in report.Rows)
        {
            var zebra = row++ % 2 == 1;
            AddCell(table, r.OrderId.ToString(), zebra, AlignCenter);
            AddCell(table, r.OrderDate.ToString("dd.MM.yyyy HH:mm"), zebra);
            AddCell(table, string.IsNullOrWhiteSpace(r.City) ? r.Customer : $"{r.Customer} ({r.City})", zebra);
            AddCell(table, StatusLabel(r.StatusName), zebra);
            AddCell(table, r.ItemsCount.ToString(), zebra, AlignCenter);
            AddCell(table, Money(r.Total), zebra, AlignRight);
        }

        document.Add(table);
        document.Add(Spacer());
        document.Add(Text($"UKUPNO: {Money(report.TotalRevenue)}", Font(13, bold: true), AlignRight));

        AddFooter(document);
        document.Close();
        return ms.ToArray();
    }

    // =========================================================
    // HELPER METHODS
    // =========================================================

    private static BaseFont? LoadUnicodeFont()
    {
        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "DejaVuSans.ttf"),
            Path.Combine(AppContext.BaseDirectory, "wwwroot", "fonts", "DejaVuSans.ttf"),
            @"C:\Windows\Fonts\arial.ttf",
            "/System/Library/Fonts/Supplemental/Arial.ttf",
            "/Library/Fonts/Arial.ttf",
            "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"
        };

        foreach (var path in candidates)
        {
            if (!File.Exists(path)) continue;
            try
            {
                return BaseFont.CreateFont(path, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            }
            catch
            {
                // try the next one
            }
        }

        return null;
    }

    private static iText.Font Font(float size, bool bold = false, iText.BaseColor? color = null)
    {
        var baseFont = UnicodeFont.Value;
        var fontColor = color ?? Black;

        if (baseFont is not null)
            return new iText.Font(baseFont, size, bold ? 1 : 0 /* Font.BOLD : Font.NORMAL */, fontColor);

        return iText.FontFactory.GetFont(
            bold ? iText.FontFactory.HELVETICA_BOLD : iText.FontFactory.HELVETICA, size, fontColor);
    }

    private static iText.Paragraph Text(string text, iText.Font font, int alignment = AlignLeft)
        => new(text, font) { Alignment = alignment };

    private static iText.Paragraph Spacer() => new(" ");

    private static void AddTitle(iText.Document document, string title)
    {
        document.Add(new iText.Paragraph("MediCare", Font(10, color: Brand))
        {
            Alignment = AlignCenter
        });
        document.Add(new iText.Paragraph(title, Font(18, bold: true))
        {
            Alignment = AlignCenter,
            SpacingAfter = 18
        });
    }

    private static void AddFooter(iText.Document document)
    {
        document.Add(new iText.Paragraph($"Izvještaj generisan: {DateTime.Now:dd.MM.yyyy HH:mm}", Font(9))
        {
            SpacingBefore = 20
        });
    }

    private static PdfPTable CreateTable(float[] widths)
    {
        var table = new PdfPTable(widths.Length) { WidthPercentage = 100 };
        table.SetWidths(widths);
        return table;
    }

    private static void AddHeader(PdfPTable table, params string[] titles)
    {
        foreach (var title in titles)
        {
            table.AddCell(new PdfPCell(new iText.Phrase(title, Font(10, bold: true, color: White)))
            {
                BackgroundColor = HeaderBackground,
                HorizontalAlignment = AlignCenter,
                Padding = 5
            });
        }
    }

    private static void AddCell(
        PdfPTable table,
        string text,
        bool zebra,
        int alignment = AlignLeft,
        bool bold = false)
    {
        var cell = new PdfPCell(new iText.Phrase(text, Font(9.5f, bold)))
        {
            HorizontalAlignment = alignment,
            Padding = 4
        };

        if (zebra)
            cell.BackgroundColor = ZebraBackground;

        table.AddCell(cell);
    }

    private static string Money(decimal value) => $"{value:N2} KM";

    private static string StatusLabel(string? status) =>
        status is not null && StatusLabels.TryGetValue(status, out var label) ? label : status ?? "-";

    private static string PeriodText(DateTime? from, DateTime? to) => (from, to) switch
    {
        (null, null) => "Sve narudžbe",
        (not null, null) => $"od {from:dd.MM.yyyy}",
        (null, not null) => $"do {to:dd.MM.yyyy}",
        _ => $"{from:dd.MM.yyyy} – {to:dd.MM.yyyy}"
    };
}
