using MediCare.Application.Modules.Sales.Orders.Queries.GetById;


public sealed class GetOrderByIdQueryHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
        : IRequestHandler<GetOrderByIdQuery, GetOrderByIdQueryDto>
{

    public async Task<GetOrderByIdQueryDto> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {

        Console.WriteLine($"GetOrderByIdQuery received with Id = {request.Id}");
        var order = ctx.Orders
            .Include(o => o.User) // ako treba User
            .Include(o => o.OrderItems) // ako treba stavke
            .Where(o => o.Id == request.Id);

        if (order == null)
        {
            Console.WriteLine($"Order with Id {request.Id} not found!");
            throw new MediCareNotFoundException($"Order with Id {request.Id} not found");
        }

        var currentUserEntity = await ctx.Users.Include(x => x.Role)
              .FirstOrDefaultAsync(u => u.Id == currentUser.UserId, ct);

        if (currentUserEntity == null)
            throw new Exception("Current user not found");

        // Ako nije admin, filtriraj po korisniku
        if (!string.Equals(currentUserEntity.Role.Name, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            order = order.Where(x => x.UserId == currentUser.UserId);
        }

        var dto = await order.OrderBy(x => x.OrderDate)
            .Select(x => new GetOrderByIdQueryDto
            {
                Id = x.Id,
                User = new GetByIdOrderQueryDtoUser
                {
                    UserFirstname = x.User!.FirstName,
                    UserLastname = x.User!.LastName,
                    UserAddress = x.User!.Adress,
                    UserCity = x.User!.City
                },
                OrderDate = x.OrderDate,
                StatusId = x.OrderStatusId,
                StatusName = x.OrderStatus.StatusName,
                //"x.Items" ili "ctx.OrderItems.Where(x => x.OrderId == x.Id)"
                Items = x.OrderItems.Select(i => new GetByIdOrderQueryDtoItems
                {
                    OrderId = i.Id,
                    Medicine = new GetByIdOrderQueryDtoItemMedicine
                    {
                        MedicineId = i.MedicineId,
                        MedicineName = i.Medicine!.Name,
                        MedicineCategoryName = i.Medicine!.MedicineCategory!.Name
                    },
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            }).FirstOrDefaultAsync(ct);

        if (dto == null)
        {
            throw new MediCareNotFoundException($"Order with Id {request.Id} not found.");
        }

        return dto;
    }
}
