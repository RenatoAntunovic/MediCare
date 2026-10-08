namespace MediCare.Application.Modules.Sales.Orders.Queries.Report;

public sealed class OrdersReportDto
{
    // Parameters (printed in the PDF header)
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public string? StatusName { get; init; }

    // Data
    public List<OrdersReportRowDto> Rows { get; init; } = [];
    public List<OrdersReportStatusSummaryDto> ByStatus { get; init; } = [];

    // Summary
    public int OrdersCount { get; init; }
    public int ItemsCount { get; init; }
    public decimal TotalRevenue { get; init; }
}

public sealed class OrdersReportRowDto
{
    public int OrderId { get; init; }
    public DateTime OrderDate { get; init; }
    public string Customer { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string StatusName { get; init; } = string.Empty;
    public int ItemsCount { get; init; }
    public decimal Total { get; init; }
}

public sealed class OrdersReportStatusSummaryDto
{
    public string StatusName { get; init; } = string.Empty;
    public int OrdersCount { get; init; }
    public decimal Total { get; init; }
}
