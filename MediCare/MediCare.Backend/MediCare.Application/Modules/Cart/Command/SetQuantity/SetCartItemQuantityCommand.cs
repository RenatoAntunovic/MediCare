namespace MediCare.Application.Modules.Cart.Command.SetQuantity;

/// <summary>
/// Sets (not adds) the quantity of one item in the current user's cart.
/// </summary>
public sealed class SetCartItemQuantityCommand : IRequest<SetCartItemQuantityResultDto>
{
    /// <summary>Set by the controller from the route.</summary>
    [JsonIgnore]
    public int CartItemId { get; set; }

    public int Quantity { get; set; }
}
