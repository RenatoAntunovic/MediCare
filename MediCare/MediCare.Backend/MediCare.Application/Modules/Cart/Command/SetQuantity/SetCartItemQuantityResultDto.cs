namespace MediCare.Application.Modules.Cart.Command.SetQuantity;

/// <summary>Updated values, so the UI can refresh the row without reloading the whole cart.</summary>
public sealed record SetCartItemQuantityResultDto(int CartItemId, int Quantity, decimal Price);
