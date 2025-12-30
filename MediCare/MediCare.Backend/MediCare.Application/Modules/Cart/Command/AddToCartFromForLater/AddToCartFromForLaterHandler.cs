using MediatR;
using MediCare.Application.Modules.Cart.Command.AddToCartFromForLater;
using Microsoft.EntityFrameworkCore;

public class AddToCartFromForLaterHandler : IRequestHandler<AddToCartFromForLaterCommand, bool>
{
    private readonly IAppDbContext _context;

    public AddToCartFromForLaterHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(AddToCartFromForLaterCommand request, CancellationToken cancellationToken)
    {
        var forLater = await _context.ForLater
            .Include(f => f.Medicine)
            .FirstOrDefaultAsync(f => f.Id == request.ForLaterId && f.UserId == request.UserId, cancellationToken);

        if (forLater == null || forLater.Medicine == null)
            return false; 

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        if (cart == null)
        {
            cart = new Carts { UserId = request.UserId };
            await _context.Carts.AddAsync(cart, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken); 
        }

        // 3️⃣ Provjeri da li već postoji stavka u cartu
        var existingItem = cart.CartItems.FirstOrDefault(ci => ci.MedicineId == forLater.MedicineId);

        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
            existingItem.SetPriceFromMedicine();
        }
        else
        {
            var cartItem = new CartItems
            {
                CartId = cart.Id,
                MedicineId = forLater.MedicineId,
                Quantity = request.Quantity,
                Medicine = forLater.Medicine
            };
            cartItem.SetPriceFromMedicine();
            cart.CartItems.Add(cartItem);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
