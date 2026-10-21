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

        // If not admin, filter by user
        if (!string.Equals(currentUserEntity.Role.Name, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            q = q.Where(x => x.UserId == currentUser.UserId);
        }
        var searchTerm = request.Search?.Trim().ToLower() ?? string.Empty;

        if (!string.IsNullOrEmpty(searchTerm))
        {
            q = q.Where(x =>
                x.User.FirstName.ToLower().Contains(searchTerm) ||
                x.User.LastName.ToLower().Contains(searchTerm) ||
                x.User.City.ToLower().Contains(searchTerm) ||
                x.Id.ToString().Contains(searchTerm));
        }

        var projectedQuery = q.OrderByDescending(x => x.OrderDate).ThenByDescending(x => x.Id)
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
                // OrderItems.Price already holds the TOTAL for the line (Medicine.Price * Quantity), so it must not be multiplied again
                TotalAmount = x.OrderItems.Sum(i => i.Price),
                OrderDate = x.OrderDate,
                StatusId = x.OrderStatusId,
                StatusName = x.OrderStatus.StatusName
            });

        return await PageResult<ListOrdersQueryDto>.FromQueryableAsync(projectedQuery, request.Paging, ct);
    }
}
