namespace MediCare.Application.Modules.Sales.Orders.Queries.ListWithItems;

public sealed class ListOrdersWithItemsQuery : BasePagedQuery<ListOrdersWithItemsQueryDto>
{
    public string? Search { get; init; }

    /// <summary>Optional order status filter (OrderStatus.Id), applied before counting and paging.</summary>
    public int? StatusId { get; init; }
}
