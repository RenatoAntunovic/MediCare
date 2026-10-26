namespace MediCare.Application.Modules.Sales.Orders.Queries.MyReport;

public sealed class MyOrdersReportDto
{
    // Parameters (printed in the PDF header)
    public string CustomerName { get; init; } = string.Empty;
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public string? StatusName { get; init; }
    public bool IncludeItems { get; init; }

    // Data
    public List<MyOrdersReportOrderDto> Orders { get; init; } = [];

    // Summary
    public int OrdersCount { get; init; }
    public int ItemsCount { get; init; }
    public decimal TotalSpent { get; init; }
    public decimal AverageOrder { get; init; }
    public string? TopMedicine { get; init; }
}

public sealed class MyOrdersReportOrderDto
{
    public int OrderId { get; init; }
    public DateTime OrderDate { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public List<MyOrdersReportItemDto> Items { get; init; } = [];
}

public sealed class MyOrdersReportItemDto
{
    public string MedicineName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal LineTotal { get; init; }
}