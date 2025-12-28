public sealed class GetOrderByIdQueryDto
{
    public required int Id { get; init; }
    public required GetByIdOrderQueryDtoUser User { get; init; }
    public required DateTime OrderDate { get; set; }
    public required int StatusId { get; set; }
    public required string StatusName { get; set; }
    public required List<GetByIdOrderQueryDtoItems> Items { get; set; }

}
public sealed class GetByIdOrderQueryDtoUser
{
    public required string UserFirstname { get; set; }
    public required string UserLastname { get; set; }
    public required string UserAddress { get; set; }//todo
    public required string UserCity { get; set; }//todoMedicine
}

public class GetByIdOrderQueryDtoItems
{
    public required int OrderId { get; set; }
    public  Orders Order { get; set; }
    public  int MedicineId { get; set; }
    public required GetByIdOrderQueryDtoItemMedicine Medicine { get; set; }
    public required int Quantity { get; set; }
    public required decimal Price { get; set; }

}

public class GetByIdOrderQueryDtoItemMedicine
{
    public required int MedicineId { get; set; }
    public required string MedicineName { get; set; }
    public required string MedicineCategoryName { get; set; }

}