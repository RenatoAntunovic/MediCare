using MediCare.Application.Modules.Sales.Orders.Commands.Create;
using MediCare.Domain.Entities.HospitalRecords;




    public class CreateOrderCommandHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
        : IRequestHandler<CreateOrderCommand, int>
    {
        public async Task<int> Handle(CreateOrderCommand request, CancellationToken ct)
        {
            #region Create order and set basic properties

            var order = new Orders
            {
                UserId = currentUser.UserId!.Value,
                OrderDate = DateTime.UtcNow,
                TotalPrice = 0m,
                OrderStatusId = 1 // npr. Draft / Created
            };

            ctx.Orders.Add(order);

            #endregion

            #region Load medicines from DB and prepare map

            List<int> medicineIds = request.Items
                .Select(i => i.MedicineId)
                .ToList();

            List<Medicine> medicines = await ctx.Medicine
                .Where(m => medicineIds.Contains(m.Id))
                .AsNoTracking()
                .ToListAsync(ct);

            Dictionary<int, Medicine> medicineMap = medicines
                .ToDictionary(m => m.Id);

            #endregion

            #region Create order items and calculate totals

            foreach (var item in request.Items)
            {
                Medicine? medicine = medicineMap.GetValueOrDefault(item.MedicineId);

                if (medicine is null)
                {
                    throw new ValidationException($"Invalid MedicineId: {item.MedicineId}");
                }

                if (medicine.isEnabled == false) // Ako imaš Enabled flag
                {
                    throw new ValidationException($"Medicine {medicine.Name} is disabled.");
                }

                decimal price = RoundMoney(medicine.Price * item.Quantity);

                var orderItem = new OrderItems
                {
                    Order = order,
                    MedicineId = item.MedicineId,
                    Quantity = item.Quantity,
                    Price = price
                };

                ctx.OrderItems.Add(orderItem);

                order.TotalPrice += RoundMoney(price);
            }

            #endregion

            await ctx.SaveChangesAsync(ct);

            return order.Id;
        }

        private static decimal RoundMoney(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }

