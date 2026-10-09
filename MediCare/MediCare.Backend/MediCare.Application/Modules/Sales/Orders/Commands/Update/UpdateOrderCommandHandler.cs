using MediCare.Domain.Entities.HospitalRecords;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Application.Modules.Sales.Orders.Commands.Update;

public class UpdateOrderCommandHandler(IAppDbContext db, IAppCurrentUser currentUser)
    : IRequestHandler<UpdateOrderCommand, int>
{
    public async Task<int> Handle(UpdateOrderCommand request, CancellationToken ct)
    {
        #region Load the order and check permissions
        // Only admins can edit orders (checked on the current user, not the order owner)
        if (!currentUser.IsAdmin)
            throw new UnauthorizedAccessException("You are not allowed to edit this order.");

        var order = await db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new MediCareNotFoundException($"Order (ID={request.Id}) not found.");

        order.TotalPrice = 0m;
        #endregion

        #region Brisanje stavki koje nisu u requestu
        var itemsToDelete = order.OrderItems
            .Where(oi => request.Items.All(ri => ri.MedicineId != oi.MedicineId))
            .ToList();

        db.OrderItems.RemoveRange(itemsToDelete);
        #endregion

        #region Dohvati sve medicine iz baze
        var medicineIds = request.Items.Select(i => i.MedicineId).ToList();

        var medicines = await db.Medicine
            .Where(m => medicineIds.Contains(m.Id))
            .ToListAsync(ct);

        var medicineMap = medicines.ToDictionary(m => m.Id);
        #endregion

        #region Update ili insert stavki
        foreach (var item in request.Items)
        {
            if (!medicineMap.TryGetValue(item.MedicineId, out var medicine))
            {
                throw new KeyNotFoundException($"Medicine (ID={item.MedicineId}) nije pronađena.");
            }

            var existingItem = order.OrderItems
                .FirstOrDefault(oi => oi.MedicineId == item.MedicineId);

            if (existingItem == null)
            {
                // Novi item
                var newItem = new OrderItems
                {
                    Order = order,
                    MedicineId = item.MedicineId,
                    Quantity = item.Quantity,
                    Price = medicine.Price * item.Quantity
                };
                db.OrderItems.Add(newItem);
                order.TotalPrice += newItem.Price;
            }
            else
            {
                // Update postojeće stavke
                existingItem.Quantity = item.Quantity;
                existingItem.SetPriceFromMedicine();
                order.TotalPrice += existingItem.Price;
            }
        }
        #endregion

        await db.SaveChangesAsync(ct);

        return order.Id;
    }
}
