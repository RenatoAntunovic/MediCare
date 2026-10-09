using MediCare.Application.Common.Exceptions;
using MediCare.Application.Modules.Cart.Command.AddToCart;

namespace MediCare.Tests.UnitTests;

public class CartTests
{
    [Fact]
    public async Task AddToCart_NewMedicine_CreatesCartItemWithTotalPrice()
    {
        // Arrange
        using var db = TestDb.Create();
        var medicine = await TestDb.AddMedicineAsync(db, price: 5.50m);
        var handler = new AddToCartCommandHandler(db, new FakeCurrentUser(1));

        // Act
        await handler.Handle(new AddToCartCommand { MedicineId = medicine.Id, Quantity = 3 }, CancellationToken.None);

        // Assert – one item, price is the line total (5.50 x 3)
        var item = await db.CartItems.SingleAsync();
        Assert.Equal(3, item.Quantity);
        Assert.Equal(16.50m, item.Price);
    }

    [Fact]
    public async Task AddToCart_SameMedicineTwice_IncreasesQuantityInsteadOfDuplicating()
    {
        // Arrange
        using var db = TestDb.Create();
        var medicine = await TestDb.AddMedicineAsync(db, price: 2.00m);
        var handler = new AddToCartCommandHandler(db, new FakeCurrentUser(1));

        // Act
        await handler.Handle(new AddToCartCommand { MedicineId = medicine.Id, Quantity = 1 }, CancellationToken.None);
        await handler.Handle(new AddToCartCommand { MedicineId = medicine.Id, Quantity = 2 }, CancellationToken.None);

        // Assert – still one cart item, quantity 3, price recalculated
        var item = await db.CartItems.SingleAsync();
        Assert.Equal(3, item.Quantity);
        Assert.Equal(6.00m, item.Price);
    }

    [Fact]
    public async Task AddToCart_DisabledMedicine_ThrowsBusinessRuleException()
    {
        // Arrange
        using var db = TestDb.Create();
        var medicine = await TestDb.AddMedicineAsync(db, price: 3.00m, isEnabled: false);
        var handler = new AddToCartCommandHandler(db, new FakeCurrentUser(1));

        // Act + Assert
        await Assert.ThrowsAsync<MediCareBusinessRuleException>(() =>
            handler.Handle(new AddToCartCommand { MedicineId = medicine.Id, Quantity = 1 }, CancellationToken.None));
        Assert.Empty(db.CartItems);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void AddToCartValidator_InvalidQuantity_IsRejected(int quantity)
    {
        var validator = new AddToCartCommandValidator();

        var result = validator.Validate(new AddToCartCommand { MedicineId = 1, Quantity = quantity });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AddToCartCommand.Quantity));
    }
}