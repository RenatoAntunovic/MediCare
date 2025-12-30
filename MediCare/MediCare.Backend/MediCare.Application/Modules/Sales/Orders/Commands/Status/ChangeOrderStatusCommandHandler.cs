using MediCare.Application.Modules.Sales.Orders.Commands.Status;

public class ChangeOrderStatusCommandHandler(IAppDbContext db)
    : IRequestHandler<ChangeOrderStatusCommand>
{
    public async Task Handle(ChangeOrderStatusCommand request, CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.OrderStatus)
            .FirstOrDefaultAsync(o => o.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Order with Id {request.Id} not found.");

        var newStatus = await db.OrderStatus
            .FirstOrDefaultAsync(s => s.Id == request.NewStatusId, ct)
            ?? throw new KeyNotFoundException($"OrderStatus with Id {request.NewStatusId} not found.");

        order.OrderStatusId = newStatus.Id;
        order.OrderStatus = newStatus;

        await db.SaveChangesAsync(ct);
    }


}
