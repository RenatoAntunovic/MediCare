namespace MediCare.Application.Modules.Sales.Orders.Queries.MyReport;

public sealed class MyOrdersReportQueryHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
    : IRequestHandler<MyOrdersReportQuery, MyOrdersReportDto>
{
    public async Task<MyOrdersReportDto> Handle(MyOrdersReportQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();

        // Only the current user's orders
        var q = ctx.Orders.AsNoTracking().Where(o => o.UserId == userId);

        if (request.From.HasValue)
        {
            var from = request.From.Value.Date;
            q = q.Where(o => o.OrderDate >= from);
        }

        if (request.To.HasValue)
        {
            // "to" is inclusive – take everything up to the end of that day
            var toExclusive = request.To.Value.Date.AddDays(1);
            q = q.Where(o => o.OrderDate < toExclusive);
        }

        if (request.StatusId.HasValue)
        {
            var statusId = request.StatusId.Value;
            q = q.Where(o => o.OrderStatusId == statusId);
        }

        var orders = await q
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new MyOrdersReportOrderDto
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate,
                StatusName = o.OrderStatus.StatusName,
                Total = o.TotalPrice,
                Items = o.OrderItems.Select(i => new MyOrdersReportItemDto
                {
                    MedicineName = i.Medicine.Name,
                    Quantity = i.Quantity,
                    LineTotal = i.Price // OrderItems.Price is already the line total
                }).ToList()
            })
            .ToListAsync(ct);

        string? statusName = null;
        if (request.StatusId.HasValue)
        {
            statusName = await ctx.OrderStatus
                .Where(s => s.Id == request.StatusId.Value)
                .Select(s => s.StatusName)
                .FirstOrDefaultAsync(ct)
                ?? throw new MediCareNotFoundException("Odabrani status ne postoji.");
        }

        var customerName = await ctx.Users
            .Where(u => u.Id == userId)
            .Select(u => u.FirstName + " " + u.LastName)
            .FirstOrDefaultAsync(ct) ?? string.Empty;

        var allItems = orders.SelectMany(o => o.Items).ToList();

        // Most purchased medicine by quantity (null if there are no orders)
        var topMedicine = allItems
            .GroupBy(i => i.MedicineName)
            .Select(g => new { Name = g.Key, Quantity = g.Sum(i => i.Quantity) })
            .OrderByDescending(x => x.Quantity)
            .FirstOrDefault();

        var totalSpent = orders.Sum(o => o.Total);

        return new MyOrdersReportDto
        {
            CustomerName = customerName,
            From = request.From?.Date,
            To = request.To?.Date,
            StatusName = statusName,
            IncludeItems = request.IncludeItems,
            Orders = orders,
            OrdersCount = orders.Count,
            ItemsCount = allItems.Sum(i => i.Quantity),
            TotalSpent = totalSpent,
            AverageOrder = orders.Count == 0 ? 0 : Math.Round(totalSpent / orders.Count, 2),
            TopMedicine = topMedicine is null ? null : $"{topMedicine.Name} ({topMedicine.Quantity} kom.)"
        };
    }
}