using MedicineEntity = MediCare.Domain.Entities.HospitalRecords.Medicine;

namespace MediCare.Application.Modules.Cart.Common;

/// <summary>
/// Cart rules in ONE place – used when adding from the medicine list/detail, from favourites and from "for later".
/// </summary>
public static class CartRules
{
    /// <summary>Total quantity of one medicine in the cart (not per add).</summary>
    public const int MaxQuantityPerMedicine = 100;

    /// <summary>
    /// Adds the medicine to the user's cart (creating the cart if needed) and returns the cart item.
    /// Disabled medicine → 409; total over the limit → 409. Caller saves the changes.
    /// </summary>
    public static async Task<CartItems> AddAsync(
        IAppDbContext db, int userId, MedicineEntity medicine, int quantity, CancellationToken ct)
    {
        if (!medicine.isEnabled)
            throw new MediCareBusinessRuleException("medicine.disabled", "Ovaj lijek trenutno nije dostupan.");

        var cart = await db.Carts.FirstOrDefaultAsync(c => c.UserId == userId, ct);
        if (cart == null)
        {
            cart = new Carts { UserId = userId };
            db.Carts.Add(cart);
            await db.SaveChangesAsync(ct); // so we get the CartId
        }

        var item = await db.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.MedicineId == medicine.Id, ct);

        var total = (item?.Quantity ?? 0) + quantity;
        if (total > MaxQuantityPerMedicine)
        {
            var left = MaxQuantityPerMedicine - (item?.Quantity ?? 0);
            throw new MediCareBusinessRuleException("cart.quantityLimit", left > 0
                ? $"U korpi može biti najviše {MaxQuantityPerMedicine} komada jednog lijeka. Možete dodati još {left}."
                : $"U korpi već imate najviše dozvoljenih {MaxQuantityPerMedicine} komada ovog lijeka.");
        }

        if (item == null)
        {
            item = new CartItems { CartId = cart.Id, MedicineId = medicine.Id };
            db.CartItems.Add(item);
        }

        item.Quantity = total;
        item.Price = medicine.Price * total; // line total at the current price

        return item;
    }
}
