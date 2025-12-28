using MediCare.Application.Modules.Cart.Command.Checkout;
using MediCare.Application.Modules.FCM;

public class CheckoutOrderCommandHandler : IRequestHandler<CheckoutOrderCommand,CheckoutOrderResponseDto>
{
    private readonly IAppDbContext _context;

    public CheckoutOrderCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<CheckoutOrderResponseDto> Handle(CheckoutOrderCommand command,CancellationToken cancellationToken)
    {
        Console.WriteLine($"CHECKOUT command.UserId: {command.UserId}");
        // Dohvati korpu
        var cart = await _context.Carts
               .Include(c => c.CartItems)
               .ThenInclude(ci => ci.Medicine)
               .FirstOrDefaultAsync(c => c.UserId == command.UserId, cancellationToken);

        if (cart == null || !cart.CartItems.Any())
            throw new Exception("Korpa je prazna");

        // Kreiraj narudžbu
        var order = new Orders
        {
            UserId = command.UserId,
            OrderDate = DateTime.Now,
            OrderStatusId = 1
        };

        var user = await _context.Users.Include(x=>x.Role).
            FirstOrDefaultAsync(x=>x.Id  == command.UserId,cancellationToken);

        Console.WriteLine(user == null ? "Korisnik nije pronađen" : $"Korisnik pronađen: {user.Id}, RoleId: {user.RoleId}");

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

        string fcmToken = null;
        SaveFcmTokenHandler.TryGetToken(command.UserId, out var token);
        fcmToken = token;

        // Vrati response DTO
        return new CheckoutOrderResponseDto
        {
            OrderId = order.Id,
            TotalPrice = order.TotalPrice,
            UserFcmToken = fcmToken ?? "dummy_token_123"
        };
    }
}
