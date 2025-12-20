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

        // Ograničenja prijelaza po ID-ju
        ValidateStatusTransition(order.OrderStatusId, newStatus.Id);

        order.OrderStatusId = newStatus.Id;
        order.OrderStatus = newStatus;

        await db.SaveChangesAsync(ct);
    }

    private static void ValidateStatusTransition(int currentId, int nextId)
    {
        // Primjer: ID-jevi statusa iz baze4
        // 1 = Draft, 2 = Confirmed, 3 = Paid, 4 = Completed, 5 = Cancelled

        var validTransitions = new Dictionary<int, int[]>
        {
       { 6, new[] { 3, 2, 7 } },    // Draft -> Pending, Processing, Cancelled
        { 3, new[] { 2, 1, 4, 7 } }, // Pending -> Processing, Paid, Failed, Cancelled
        { 2, new[] { 1, 4, 7 } },    // Processing -> Paid, Failed, Cancelled
        { 1, new[] { 5 } },           // Paid -> Completed
        { 4, new[] { 2, 7 } },       // Failed -> Processing, Cancelled
        { 5, Array.Empty<int>() },    // Completed -> ništa
        { 7, Array.Empty<int>() }
        };

        if (!validTransitions.ContainsKey(currentId))
            throw new InvalidOperationException($"Unknown current status ID: {currentId}");

        if (!validTransitions[currentId].Contains(nextId))
            throw new InvalidOperationException(
                $"Invalid status transition from ID {currentId} to ID {nextId}. " +
                $"Allowed: {string.Join(", ", validTransitions[currentId])}");
    }
}
