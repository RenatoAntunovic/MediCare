namespace MediCare.Application.Modules.Sales.Orders.Queries.List;

public sealed class ListOrdersQueryDto
{
    public required int Id { get; init; }
    public required ListOrdersQueryDtoUser User { get; init; }
    public required DateTime OrderDate { get; set; }
    public required OrderStatus Status { get; set; }

}
public sealed class ListOrdersQueryDtoUser
{
    public required string UserFirstname { get; set; }
    public required string UserLastname { get; set; }
    public required string UserAddress { get; set; }//todo
    public required string UserCity { get; set; }//todo
}
