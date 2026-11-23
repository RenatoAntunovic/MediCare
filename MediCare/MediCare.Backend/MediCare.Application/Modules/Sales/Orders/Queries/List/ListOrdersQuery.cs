namespace MediCare.Application.Modules.Sales.Orders.Queries.List;

public sealed class ListOrdersQuery : BasePagedQuery<ListOrdersQueryDto>
{
    public string? Search { get; init; }

    /// <summary>Optional order status filter (OrderStatus.Id), applied before counting and paging.</summary>
    public int? StatusId { get; init; }
}
