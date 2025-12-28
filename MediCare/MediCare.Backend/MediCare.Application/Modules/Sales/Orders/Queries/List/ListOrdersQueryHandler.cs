using MediCare.Application.Modules.Sales.Orders.Queries.List;

public sealed class ListOrdersQueryHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
        : IRequestHandler<ListOrdersQuery, PageResult<ListOrdersQueryDto>>
{

    public async Task<PageResult<ListOrdersQueryDto>> Handle(ListOrdersQuery request, CancellationToken ct)
    {
        var q = ctx.Orders.AsNoTracking();

        var currentUserEntity = await ctx.Users.Include(x => x.Role)
            .FirstOrDefaultAsync(u => u.Id == currentUser.UserId, ct);

        if (currentUserEntity == null)
            throw new Exception("Current user not found");

        // Ako nije admin, filtriraj po korisniku
        if (!string.Equals(currentUserEntity.Role.Name, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            q = q.Where(x => x.UserId == currentUser.UserId);
        }
        var searchTerm = request.Search?.Trim().ToLower() ?? string.Empty;



        var projectedQuery = q.OrderBy(x => x.OrderDate)
            .Select(x => new ListOrdersQueryDto
            {
                Id = x.Id,
                User = new ListOrdersQueryDtoUser
                {
                    UserFirstname = x.User.FirstName,
                    UserLastname = x.User.LastName,
                    UserAddress = x.User.Adress,
                    UserCity = x.User.City
                },
                TotalAmount = x.OrderItems.Sum(i => i.Price * i.Quantity),
                OrderDate = x.OrderDate,
                StatusId = x.OrderStatusId,
                StatusName = x.OrderStatus.StatusName
            });

        return await PageResult<ListOrdersQueryDto>.FromQueryableAsync(projectedQuery, request.Paging, ct);
    }
}
