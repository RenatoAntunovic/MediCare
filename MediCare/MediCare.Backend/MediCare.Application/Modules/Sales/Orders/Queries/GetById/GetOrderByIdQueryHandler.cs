using MediCare.Application.Modules.Sales.Orders.Queries.GetById;


public sealed class GetOrderByIdQueryHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
        : IRequestHandler<GetOrderByIdQuery, GetOrderByIdQueryDto>
{

    public async Task<GetOrderByIdQueryDto> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        var q = ctx.Orders
            .Where(c => c.Id == request.Id);

        if (!currentUser.IsAdmin)
        {
            q = q.Where(x => x.UserId == currentUser.UserId);
        }

        var dto = await q.OrderBy(x => x.OrderDate)
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
                Status = x.OrderStatus,
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
