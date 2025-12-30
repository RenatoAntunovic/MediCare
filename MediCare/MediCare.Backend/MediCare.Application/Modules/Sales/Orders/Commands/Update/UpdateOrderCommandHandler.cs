using MediCare.Domain.Entities.HospitalRecords;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Application.Modules.Sales.Orders.Commands.Update;

public class UpdateOrderCommandHandler(IAppDbContext db)
    : IRequestHandler<UpdateOrderCommand, int>
{
    public async Task<int> Handle(UpdateOrderCommand request, CancellationToken ct)
    {
        #region Dohvati postojeću narudžbu s korisnikom i ulogom
        var orderQuery = db.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.User)
                .ThenInclude(u => u.Role) // dohvatimo ulogu korisnika
            .Where(x => x.Id == request.Id);

        var order = await orderQuery.FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException($"Order (ID={request.Id}) nije pronađen.");

        // Provjera prava: samo korisnik s određenom ulogom može mijenjati tu narudžbu
        if (order.User?.Role?.Name != "Admin")
        {
            throw new UnauthorizedAccessException("Nemate prava mijenjati ovu narudžbu.");
        }

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
