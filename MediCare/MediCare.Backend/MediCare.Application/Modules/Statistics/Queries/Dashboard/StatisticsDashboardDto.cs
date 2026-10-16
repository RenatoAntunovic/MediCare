namespace MediCare.Application.Modules.Statistics.Queries.Dashboard;

public sealed class StatisticsDashboardDto
{
    // Parameters
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }

    // Summary tiles
    public decimal OrdersRevenue { get; init; }
    public decimal ReservationsRevenue { get; init; }
    public int OrdersCount { get; init; }
    public int ReservationsCount { get; init; }
    public int NewUsersCount { get; init; }
    public int LowStockCount { get; init; }

    // Charts
    public List<StatisticsMonthDto> ByMonth { get; init; } = [];
    public List<StatisticsCountDto> OrdersByStatus { get; init; } = [];
    public List<StatisticsCountDto> ReservationsByStatus { get; init; } = [];
    public List<StatisticsCountDto> ReservationsByTreatment { get; init; } = [];
    public List<StatisticsCountDto> ReservationsByWeekday { get; init; } = [];
    public List<StatisticsTopMedicineDto> TopMedicines { get; init; } = [];
    public List<StatisticsStockDto> LowStock { get; init; } = [];
}

/// <summary>One month on the time axis. Revenue counts only orders/reservations that are not DRAFT or CANCELLED.</summary>
public sealed class StatisticsMonthDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal OrdersRevenue { get; init; }
    public decimal ReservationsRevenue { get; init; }
    public int OrdersCount { get; init; }
    public int ReservationsCount { get; init; }
}

public sealed class StatisticsCountDto
{
    public string Label { get; init; } = string.Empty;
    public int Count { get; init; }
}

public sealed class StatisticsTopMedicineDto
{
    public string Name { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal Revenue { get; init; }
}

public sealed class StatisticsStockDto
{
    public string Name { get; init; } = string.Empty;
    public int QuantityInStock { get; init; }
}
