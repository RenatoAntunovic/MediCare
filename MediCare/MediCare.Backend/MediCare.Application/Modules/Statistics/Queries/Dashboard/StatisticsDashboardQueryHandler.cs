namespace MediCare.Application.Modules.Statistics.Queries.Dashboard;

public sealed class StatisticsDashboardQueryHandler(IAppDbContext ctx)
    : IRequestHandler<StatisticsDashboardQuery, StatisticsDashboardDto>
{
    // Orders/reservations in these statuses are not real income
    private static readonly string[] NotCountedStatuses = ["DRAFT", "CANCELLED"];

    public async Task<StatisticsDashboardDto> Handle(StatisticsDashboardQuery request, CancellationToken ct)
    {
        DateTime? from = request.From?.Date;
        // "to" is inclusive – take everything up to the end of that day
        DateTime? toExclusive = request.To?.Date.AddDays(1);

        // ---- Orders (light rows, aggregated in memory like the orders report) ----
        var ordersQuery = ctx.Orders.AsNoTracking();
        if (from.HasValue) ordersQuery = ordersQuery.Where(o => o.OrderDate >= from.Value);
        if (toExclusive.HasValue) ordersQuery = ordersQuery.Where(o => o.OrderDate < toExclusive.Value);

        var orders = await ordersQuery
            .Select(o => new { o.Id, o.OrderDate, o.TotalPrice, Status = o.OrderStatus.StatusName })
            .ToListAsync(ct);

        var countedOrders = orders.Where(o => !NotCountedStatuses.Contains(o.Status)).ToList();
        var countedOrderIds = countedOrders.Select(o => o.Id).ToList();

        // ---- Reservations ----
        var reservationsQuery = ctx.Reservations.AsNoTracking();
        if (from.HasValue) reservationsQuery = reservationsQuery.Where(r => r.ReservationDate >= from.Value);
        if (toExclusive.HasValue) reservationsQuery = reservationsQuery.Where(r => r.ReservationDate < toExclusive.Value);

        var reservations = await reservationsQuery
            .Select(r => new
            {
                r.ReservationDate,
                r.Price,
                Status = r.OrderStatus.StatusName,
                Treatment = r.Treatment.ServiceName
            })
            .ToListAsync(ct);

        var countedReservations = reservations.Where(r => !NotCountedStatuses.Contains(r.Status)).ToList();

        // ---- Top medicines (from counted orders only) ----
        var topMedicines = await ctx.OrderItems.AsNoTracking()
            .Where(i => countedOrderIds.Contains(i.OrderId))
            .GroupBy(i => i.Medicine.Name)
            .Select(g => new StatisticsTopMedicineDto
            {
                Name = g.Key,
                Quantity = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.Price) // Price is already the line total
            })
            .OrderByDescending(m => m.Quantity)
            .Take(5)
            .ToListAsync(ct);

        // ---- Stock (current state, not affected by the period) ----
        var lowStock = await ctx.Inventories.AsNoTracking()
            .Where(i => i.QuantityInStock <= request.LowStockThreshold)
            .OrderBy(i => i.QuantityInStock)
            .Select(i => new StatisticsStockDto
            {
                Name = i.Medicine.Name,
                QuantityInStock = i.QuantityInStock
            })
            .ToListAsync(ct);

        // ---- New users ----
        var usersQuery = ctx.Users.AsNoTracking();
        if (from.HasValue) usersQuery = usersQuery.Where(u => u.CreatedAtUtc >= from.Value);
        if (toExclusive.HasValue) usersQuery = usersQuery.Where(u => u.CreatedAtUtc < toExclusive.Value);
        var newUsersCount = await usersQuery.CountAsync(ct);

        return new StatisticsDashboardDto
        {
            From = from,
            To = request.To?.Date,

            OrdersRevenue = countedOrders.Sum(o => o.TotalPrice),
            ReservationsRevenue = countedReservations.Sum(r => r.Price),
            OrdersCount = orders.Count,
            ReservationsCount = reservations.Count,
            NewUsersCount = newUsersCount,
            LowStockCount = lowStock.Count,

            ByMonth = BuildMonths(
                from,
                request.To?.Date,
                countedOrders.Select(o => (o.OrderDate, o.TotalPrice)).ToList(),
                countedReservations.Select(r => (r.ReservationDate, r.Price)).ToList()),

            OrdersByStatus = CountBy(orders.Select(o => o.Status)),
            ReservationsByStatus = CountBy(reservations.Select(r => r.Status)),
            ReservationsByTreatment = CountBy(reservations.Select(r => r.Treatment)),

            // Always Monday..Friday (bookable days), so the chart axis is stable
            ReservationsByWeekday = Enum.GetValues<DayOfWeek>()
                .Where(d => d is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
                .Select(d => new StatisticsCountDto
                {
                    Label = d.ToString(),
                    Count = reservations.Count(r => r.ReservationDate.DayOfWeek == d)
                })
                .ToList(),

            TopMedicines = topMedicines,
            LowStock = lowStock
        };
    }

    private static List<StatisticsCountDto> CountBy(IEnumerable<string> labels) =>
        labels
            .GroupBy(l => l)
            .Select(g => new StatisticsCountDto { Label = g.Key, Count = g.Count() })
            .OrderByDescending(c => c.Count)
            .ToList();

    /// <summary>
    /// One entry per month between the first and last month, including empty months,
    /// so the line chart doesn't skip gaps. Without a period the range comes from the data itself.
    /// </summary>
    private static List<StatisticsMonthDto> BuildMonths(
        DateTime? from,
        DateTime? to,
        List<(DateTime Date, decimal Amount)> orders,
        List<(DateTime Date, decimal Amount)> reservations)
    {
        var allDates = orders.Select(o => o.Date).Concat(reservations.Select(r => r.Date)).ToList();

        var start = from ?? (allDates.Count > 0 ? allDates.Min() : (DateTime?)null);
        var end = to ?? (allDates.Count > 0 ? allDates.Max() : (DateTime?)null);
        if (start is null || end is null)
            return [];

        var result = new List<StatisticsMonthDto>();
        var month = new DateTime(start.Value.Year, start.Value.Month, 1);
        var lastMonth = new DateTime(end.Value.Year, end.Value.Month, 1);

        while (month <= lastMonth)
        {
            var m = month;
            bool InMonth(DateTime d) => d.Year == m.Year && d.Month == m.Month;

            result.Add(new StatisticsMonthDto
            {
                Year = m.Year,
                Month = m.Month,
                OrdersRevenue = orders.Where(o => InMonth(o.Date)).Sum(o => o.Amount),
                ReservationsRevenue = reservations.Where(r => InMonth(r.Date)).Sum(r => r.Amount),
                OrdersCount = orders.Count(o => InMonth(o.Date)),
                ReservationsCount = reservations.Count(r => InMonth(r.Date))
            });

            month = month.AddMonths(1);
        }

        return result;
    }
}
