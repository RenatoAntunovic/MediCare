using MediCare.Application.Modules.Sales.Orders.Queries.List;

public sealed class ListOrdersQueryHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
        : IRequestHandler<ListOrdersQuery, PageResult<ListOrdersQueryDto>>
{

    public async Task<PageResult<ListOrdersQueryDto>> Handle(ListOrdersQuery request, CancellationToken ct)
    {
        var q = ctx.Orders.AsNoTracking();

        if (!currentUser.IsAdmin)
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
                OrderDate = x.OrderDate,
                Status = x.OrderStatus,
            });

        return await PageResult<ListOrdersQueryDto>.FromQueryableAsync(projectedQuery, request.Paging, ct);
    }
}
