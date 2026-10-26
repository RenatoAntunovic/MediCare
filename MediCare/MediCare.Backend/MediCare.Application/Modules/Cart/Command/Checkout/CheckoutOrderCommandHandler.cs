using MediCare.Application.Modules.Cart.Command.Checkout;

public class CheckoutOrderCommandHandler : IRequestHandler<CheckoutOrderCommand,CheckoutOrderResponseDto>
{
    private readonly IAppDbContext _context;
    private readonly IPushNotificationService _push;

    public CheckoutOrderCommandHandler(IAppDbContext context, IPushNotificationService push)
    {
        _context = context;
        _push = push;
    }

    public async Task<CheckoutOrderResponseDto> Handle(CheckoutOrderCommand command,CancellationToken cancellationToken)
    {
        // Get cart
        var cart = await _context.Carts
               .Include(c => c.CartItems)
               .ThenInclude(ci => ci.Medicine)
               .FirstOrDefaultAsync(c => c.UserId == command.UserId, cancellationToken);

        if (cart == null || !cart.CartItems.Any())
            throw new MediCareBusinessRuleException("cart.empty", "The cart is empty.");

        // Don't allow ordering medicines that were disabled after being added to the cart
        var disabled = cart.CartItems.Where(ci => !ci.Medicine.isEnabled).Select(ci => ci.Medicine.Name).ToList();
        if (disabled.Count > 0)
            throw new MediCareBusinessRuleException("cart.disabled-items",
                $"These medicines are no longer available: {string.Join(", ", disabled)}");

        // Create order
        var order = new Orders
        {
            UserId = command.UserId,
            OrderDate = DateTime.Now,
            OrderStatusId = 1
        };

        foreach (var cartItem in cart.CartItems)
        {
            var orderItem = new OrderItems
            {
                MedicineId = cartItem.MedicineId,
                Quantity = cartItem.Quantity,
                Medicine = cartItem.Medicine
            };
            orderItem.SetPriceFromMedicine();
            order.OrderItems.Add(orderItem);
        }

        order.TotalPrice = order.OrderItems.Sum(x => x.Price);

        _context.Orders.Add(order);
        _context.CartItems.RemoveRange(cart.CartItems);
        await _context.SaveChangesAsync(cancellationToken);

        await _push.SendToUserAsync(command.UserId,
           "Narudžba zaprimljena",
           $"Vaša narudžba #{order.Id} u iznosu od {order.TotalPrice:0.00} KM je zaprimljena.",
           cancellationToken);

        // Return response DTO
        return new CheckoutOrderResponseDto
        {
            OrderId = order.Id,
            TotalPrice = order.TotalPrice
        };
    }
}
