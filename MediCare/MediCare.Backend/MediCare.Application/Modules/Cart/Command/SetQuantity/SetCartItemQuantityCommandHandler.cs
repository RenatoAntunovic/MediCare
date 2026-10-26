namespace MediCare.Application.Modules.Cart.Command.SetQuantity;

public sealed class SetCartItemQuantityCommandHandler(IAppDbContext db, IAppCurrentUser currentUser)
    : IRequestHandler<SetCartItemQuantityCommand, SetCartItemQuantityResultDto>
{
    public async Task<SetCartItemQuantityResultDto> Handle(SetCartItemQuantityCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();

        // Only an item from the current user's own cart
        var cartItem = await db.CartItems
            .Include(ci => ci.Medicine)
            .FirstOrDefaultAsync(ci => ci.Id == request.CartItemId && ci.Cart.UserId == userId, ct)
            ?? throw new MediCareNotFoundException($"Cart item with Id {request.CartItemId} not found.");

        if (!cartItem.Medicine.isEnabled)
            throw new MediCareBusinessRuleException("medicine.disabled", "This medicine is currently unavailable.");

        cartItem.Quantity = request.Quantity;
        cartItem.SetPriceFromMedicine(); // line total = unit price x quantity

        await db.SaveChangesAsync(ct);

        return new SetCartItemQuantityResultDto(cartItem.Id, cartItem.Quantity, cartItem.Price);
    }
}