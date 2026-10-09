using iTextSharp.text.pdf;
using MediCare.Application.Modules.Sales.Orders.Queries.MyReport;
using iText = iTextSharp.text;

namespace MediCare.API.Reports;

public static partial class OrderPdfBuilder
{
    // =========================================================
    // 3) CLIENT REPORT – "My orders" (period + status + items)
    // =========================================================
    public static byte[] BuildMyOrdersReportPdf(MyOrdersReportDto report)
    {
        using var ms = new MemoryStream();
        var document = new iText.Document(iText.PageSize.A4, 40, 40, 30, 30);
        PdfWriter.GetInstance(document, ms);
        document.Open();

        AddTitle(document, "Moje narudžbe");

        // --- Parameters ---
        document.Add(Text("Parametri izvještaja", Font(12, bold: true)));
        if (!string.IsNullOrWhiteSpace(report.CustomerName))
            document.Add(Text($"Kupac: {report.CustomerName}", Font(10)));
        document.Add(Text($"Period: {PeriodText(report.From, report.To)}", Font(10)));
        document.Add(Text($"Status: {(report.StatusName is null ? "Svi statusi" : StatusLabel(report.StatusName))}", Font(10)));
        document.Add(Text($"Prikaz stavki: {(report.IncludeItems ? "Da" : "Ne")}", Font(10)));
        document.Add(Spacer());

        // --- Summary ---
        document.Add(Text("Sažetak", Font(12, bold: true)));
        var summary = CreateTable(new float[] { 3, 2.5f });
        summary.WidthPercentage = 65;
        summary.HorizontalAlignment = AlignLeft;
        AddCell(summary, "Broj narudžbi", false);
        AddCell(summary, report.OrdersCount.ToString(), false, AlignRight);
        AddCell(summary, "Kupljenih komada", true);
        AddCell(summary, report.ItemsCount.ToString(), true, AlignRight);
        AddCell(summary, "Prosječna narudžba", false);
        AddCell(summary, Money(report.AverageOrder), false, AlignRight);
        AddCell(summary, "Najčešće kupovano", true);
        AddCell(summary, report.TopMedicine ?? "-", true, AlignRight);
        AddCell(summary, "Ukupno potrošeno", false, bold: true);
        AddCell(summary, Money(report.TotalSpent), false, AlignRight, bold: true);
        document.Add(summary);
        document.Add(Spacer());

        if (report.Orders.Count == 0)
        {
            document.Add(Text("Nemate narudžbi za odabrane parametre.", Font(11)));
            AddFooter(document);
            document.Close();
            return ms.ToArray();
        }

        // --- Orders (optionally with items) ---
        document.Add(Text("Narudžbe", Font(12, bold: true)));
        var table = CreateTable(new float[] { 1, 2.4f, 2.2f, 1.3f, 2 });
        table.HeaderRows = 1; // header row repeats on every page
        AddHeader(table, "#", "Datum", "Status", "Kom.", "Iznos");

        var row = 0;
        foreach (var o in report.Orders)
        {
            var zebra = row++ % 2 == 1;
            AddCell(table, o.OrderId.ToString(), zebra, AlignCenter);
            AddCell(table, o.OrderDate.ToString("dd.MM.yyyy HH:mm"), zebra);
            AddCell(table, StatusLabel(o.StatusName), zebra);
            AddCell(table, o.Items.Sum(i => i.Quantity).ToString(), zebra, AlignCenter);
            AddCell(table, Money(o.Total), zebra, AlignRight, bold: true);

            if (report.IncludeItems && o.Items.Count > 0)
            {
                // One full-width row under the order with its items
                var lines = o.Items.Select(i => $"• {i.MedicineName} × {i.Quantity} = {Money(i.LineTotal)}");
                var itemsCell = new PdfPCell(new iText.Phrase(string.Join("\n", lines), Font(9)))
                {
                    Colspan = 5,
                    PaddingLeft = 24,
                    PaddingTop = 3,
                    PaddingBottom = 6
                };
                if (zebra) itemsCell.BackgroundColor = ZebraBackground;
                table.AddCell(itemsCell);
            }
        }

        document.Add(table);
        document.Add(Spacer());
        document.Add(Text($"UKUPNO: {Money(report.TotalSpent)}", Font(13, bold: true), AlignRight));

        AddFooter(document);
        document.Close();
        return ms.ToArray();
    }
}