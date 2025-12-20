namespace MediCare.Application.Modules.Sales.Orders.Queries.ListWithItems;

public sealed class ListOrdersWithItemsQueryHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
        : IRequestHandler<ListOrdersWithItemsQuery, PageResult<ListOrdersWithItemsQueryDto>>
{

    public async Task<PageResult<ListOrdersWithItemsQueryDto>> Handle(ListOrdersWithItemsQuery request, CancellationToken ct)
    {
        var q = ctx.Orders.AsNoTracking();

        if (!currentUser.IsAdmin)
        {
            q = q.Where(x => x.UserId == currentUser.UserId);
        }
        var searchTerm = request.Search?.Trim().ToLower() ?? string.Empty;

        var projectedQuery = q.OrderBy(x => x.OrderDate)
            .Select(x => new ListOrdersWithItemsQueryDto
            {
                OrderId = x.Id,
                User = new ListOrdersWithItemsQueryDtoUser
                {
                    UserFirstname = x.User!.FirstName,
                    UserLastname = x.User!.LastName,
                    UserAddress = x.User!.Adress,//todo: ticket no 126
                    UserCity = x.User!.City,//todo: ticket no 126
                    PhoneNumber = x.User!.PhoneNumber
                },
                OrderDate = x.OrderDate,
                OrderStatus = x.OrderStatus,
                //"x.Items" ili "ctx.OrderItems.Where(x => x.OrderId == x.Id)"
                Items = x.OrderItems.Select(i => new ListOrdersWithItemsQueryDtoItem
                {
                    Id = i.Id,
                    Medicine = new ListOrdersWithItemsQueryDtoItemMedicine
                    {
                        MedicineId = i.MedicineId,
                        MedicineName = i.Medicine!.Name,
                        MedicineCategoryName = i.Medicine.MedicineCategory.Name
                    },
                    Quantity = i.Quantity,
                    Price = i.Price,           
                }).ToList()
            });

        return await PageResult<ListOrdersWithItemsQueryDto>.FromQueryableAsync(projectedQuery, request.Paging, ct);
    }
}
