using MediCare.Application.Modules.Sales.Orders.Commands.Status;
using MediCare.Application.Modules.Notifications;

public class ChangeOrderStatusCommandHandler(IAppDbContext db, IPushNotificationService push)
    : IRequestHandler<ChangeOrderStatusCommand>
{
    public async Task Handle(ChangeOrderStatusCommand request, CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.OrderStatus)
            .FirstOrDefaultAsync(o => o.Id == request.Id, ct)
            ?? throw new MediCareNotFoundException($"Order with Id {request.Id} not found.");

        var newStatus = await db.OrderStatus
            .FirstOrDefaultAsync(s => s.Id == request.NewStatusId, ct)
            ?? throw new MediCareNotFoundException($"OrderStatus with Id {request.NewStatusId} not found.");

        // The server enforces the same transitions as the admin dialog, so a direct API call
        // cannot e.g. reopen a completed or cancelled order. Nothing is saved and no push is sent.
        if (!OrderStatusTransitions.IsAllowed(order.OrderStatus.StatusName, newStatus.StatusName))
            throw new MediCareBusinessRuleException("order.status.transition.notAllowed",
                $"Narudžba se ne može prebaciti iz statusa \"{StatusLabels.ToBosnian(order.OrderStatus.StatusName)}\" " +
                $"u status \"{StatusLabels.ToBosnian(newStatus.StatusName)}\".");

        order.OrderStatusId = newStatus.Id;
        order.OrderStatus = newStatus;

        await db.SaveChangesAsync(ct);

        await push.SendToUserAsync(order.UserId,
            "Status narudžbe promijenjen",
            $"Narudžba #{order.Id} je {StatusLabels.ToBosnian(newStatus.StatusName)}.",
            ct);
    }
}
