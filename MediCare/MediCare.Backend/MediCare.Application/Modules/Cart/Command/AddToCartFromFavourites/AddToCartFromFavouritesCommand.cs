using MediatR;

public class AddToCartFromFavouritesCommand : IRequest<bool>
{
    public int UserId { get; set; }
    public int FavouriteId { get; set; }
    public int Quantity { get; set; } = 1;

    public AddToCartFromFavouritesCommand(int userId, int favouriteId, int quantity = 1)
    {
        UserId = userId;
        FavouriteId = favouriteId;
        Quantity = quantity;
    }
}
