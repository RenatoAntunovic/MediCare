using MediatR;
using MediCare.Application.Modules.Cart.Common;
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
        // 1️⃣ Get the favourite item (only the user's own)
        var favourite = await _context.Favourites
            .Include(f => f.Medicine)
            .FirstOrDefaultAsync(f => f.Id == request.FavouriteId && f.UserId == request.UserId, cancellationToken);

        if (favourite == null || favourite.Medicine == null)
            return false;

        // 2️⃣ Same rules as everywhere else: disabled medicine and the cart limit → 409
        await CartRules.AddAsync(_context, request.UserId, favourite.Medicine, request.Quantity, cancellationToken);

        // Favourites stay – they are a bookmark, not a shopping list
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
