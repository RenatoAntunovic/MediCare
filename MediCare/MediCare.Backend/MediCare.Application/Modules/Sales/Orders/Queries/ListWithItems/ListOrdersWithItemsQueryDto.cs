public sealed class ListOrdersWithItemsQueryDto
{
    public int OrderId { get; set; }
    public required ListOrdersWithItemsQueryDtoUser User { get; init; }
    public DateTime OrderDate { get; set; }
    public Orders Order { get; set; }
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public required OrderStatus OrderStatus { get; set; }
    public required List<ListOrdersWithItemsQueryDtoItem> Items { get; set; }



}
public sealed class ListOrdersWithItemsQueryDtoUser
{
    public required string UserFirstname { get; set; }
    public required string UserLastname { get; set; }
    public required string UserAddress { get; set; }//todo
    public required string UserCity { get; set; }//todo
    public required string PhoneNumber { get; set; }
}

public class ListOrdersWithItemsQueryDtoItem
{
    public required int Id { get; set; }
    public required ListOrdersWithItemsQueryDtoItemMedicine Medicine { get; set; }
    public required decimal Quantity { get; set; }
    public required decimal Price { get; set; }

}

public class ListOrdersWithItemsQueryDtoItemMedicine
{
    public required int MedicineId { get; set; }
    public required string MedicineName { get; set; }
    public required string MedicineCategoryName { get; set; }

}