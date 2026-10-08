namespace MediCare.Application.Modules.Sales.Orders.Queries.Report;

public sealed class OrdersReportQueryHandler(IAppDbContext ctx)
    : IRequestHandler<OrdersReportQuery, OrdersReportDto>
{
    public async Task<OrdersReportDto> Handle(OrdersReportQuery request, CancellationToken ct)
    {
        var q = ctx.Orders.AsNoTracking();

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

        var rows = await q
            .OrderBy(o => o.OrderDate)
            .Select(o => new OrdersReportRowDto
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate,
                Customer = o.User.FirstName + " " + o.User.LastName,
                City = o.User.City,
                StatusName = o.OrderStatus.StatusName,
                ItemsCount = o.OrderItems.Sum(i => i.Quantity),
                Total = o.TotalPrice   // order total – do NOT compute Price * Quantity
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

        var byStatus = rows
            .GroupBy(r => r.StatusName)
            .Select(g => new OrdersReportStatusSummaryDto
            {
                StatusName = g.Key,
                OrdersCount = g.Count(),
                Total = g.Sum(r => r.Total)
            })
            .OrderByDescending(s => s.OrdersCount)
            .ToList();

        return new OrdersReportDto
        {
            From = request.From?.Date,
            To = request.To?.Date,
            StatusName = statusName,
            Rows = rows,
            ByStatus = byStatus,
            OrdersCount = rows.Count,
            ItemsCount = rows.Sum(r => r.ItemsCount),
            TotalRevenue = rows.Sum(r => r.Total)
        };
    }
}
