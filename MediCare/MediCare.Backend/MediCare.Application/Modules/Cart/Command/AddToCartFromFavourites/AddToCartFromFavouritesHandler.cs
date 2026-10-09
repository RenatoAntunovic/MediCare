using MediatR;
using Microsoft.EntityFrameworkCore;

public class AddToCartFromFavouritesHandler : IRequestHandler<AddToCartFromFavouritesCommand, bool>
{
    private readonly IAppDbContext _context;

    public AddToCartFromFavouritesHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(AddToCartFromFavouritesCommand request, CancellationToken cancellationToken)
    {
        // 1️⃣ Get the favourite item
        var favourite = await _context.Favourites
            .Include(f => f.Medicine)
            .FirstOrDefaultAsync(f => f.Id == request.FavouriteId && f.UserId == request.UserId, cancellationToken);

        if (favourite == null || favourite.Medicine == null)
            return false; // or throw new Exception("Favourite or medicine does not exist");

        // 2️⃣ Get or create the cart for the user
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        if (cart == null)
        {
            cart = new Carts { UserId = request.UserId };
            await _context.Carts.AddAsync(cart, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken); // so we get the CartId
        }

        // 3️⃣ Check whether the item is already in the cart
        var existingItem = cart.CartItems.FirstOrDefault(ci => ci.MedicineId == favourite.MedicineId);

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
                MedicineId = favourite.MedicineId,
                Quantity = request.Quantity,
                Medicine = favourite.Medicine
            };
            cartItem.SetPriceFromMedicine();
            cart.CartItems.Add(cartItem);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
