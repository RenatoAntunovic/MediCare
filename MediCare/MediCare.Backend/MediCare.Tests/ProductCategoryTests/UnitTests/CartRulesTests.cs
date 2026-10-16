using MediCare.Application.Common.Exceptions;
using MediCare.Application.Modules.Cart.Command.AddToCart;
using MediCare.Application.Modules.Cart.Command.AddToCartFromForLater;
using MediCare.Application.Modules.Cart.Queries;
using MediCare.Domain.Entities.HospitalRecords;

namespace MediCare.Tests.UnitTests;

/// <summary>Cart rules that must hold no matter where the medicine is added from.</summary>
public class CartRulesTests
{
    [Fact]
    public async Task AddToCart_TotalOver100_ThrowsAndKeepsQuantity()
    {
        // Arrange – 60 already in the cart
        using var db = TestDb.Create();
        var medicine = await TestDb.AddMedicineAsync(db, price: 1m);
        var handler = new AddToCartCommandHandler(db, new FakeCurrentUser(1));
        await handler.Handle(new AddToCartCommand { MedicineId = medicine.Id, Quantity = 60 }, CancellationToken.None);

        // Act + Assert – another 60 would be 120
        await Assert.ThrowsAsync<MediCareBusinessRuleException>(() =>
            handler.Handle(new AddToCartCommand { MedicineId = medicine.Id, Quantity = 60 }, CancellationToken.None));

        Assert.Equal(60, (await db.CartItems.SingleAsync()).Quantity);
    }

    [Fact]
    public async Task AddToCart_ExactlyUpTo100_IsAllowed()
    {
        using var db = TestDb.Create();
        var medicine = await TestDb.AddMedicineAsync(db, price: 1m);
        var handler = new AddToCartCommandHandler(db, new FakeCurrentUser(1));

        await handler.Handle(new AddToCartCommand { MedicineId = medicine.Id, Quantity = 60 }, CancellationToken.None);
        await handler.Handle(new AddToCartCommand { MedicineId = medicine.Id, Quantity = 40 }, CancellationToken.None);

        Assert.Equal(100, (await db.CartItems.SingleAsync()).Quantity);
    }

    [Fact]
    public async Task AddFromFavourites_DisabledMedicine_Throws()
    {
        // Arrange
        using var db = TestDb.Create();
        var medicine = await TestDb.AddMedicineAsync(db, price: 2m, isEnabled: false);
        var favourite = new Favourites { UserId = 1, MedicineId = medicine.Id };
        db.Favourites.Add(favourite);
        await db.SaveChangesAsync();

        // Act + Assert
        await Assert.ThrowsAsync<MediCareBusinessRuleException>(() =>
            new AddToCartFromFavouritesHandler(db).Handle(new AddToCartFromFavouritesCommand(1, favourite.Id), CancellationToken.None));
        Assert.Empty(db.CartItems);
    }

    [Fact]
    public async Task AddFromForLater_DisabledMedicine_Throws()
    {
        using var db = TestDb.Create();
        var medicine = await TestDb.AddMedicineAsync(db, price: 2m, isEnabled: false);
        var saved = new ForLater { UserId = 1, MedicineId = medicine.Id };
        db.ForLater.Add(saved);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<MediCareBusinessRuleException>(() =>
            new AddToCartFromForLaterHandler(db).Handle(new AddToCartFromForLaterCommand(1, saved.Id, 1), CancellationToken.None));
        Assert.Empty(db.CartItems);
    }

    [Fact]
    public async Task AddFromForLater_MovesItemFromListToCart()
    {
        // Arrange
        using var db = TestDb.Create();
        var medicine = await TestDb.AddMedicineAsync(db, price: 2m);
        var saved = new ForLater { UserId = 1, MedicineId = medicine.Id };
        db.ForLater.Add(saved);
        await db.SaveChangesAsync();

        // Act
        var ok = await new AddToCartFromForLaterHandler(db)
            .Handle(new AddToCartFromForLaterCommand(1, saved.Id, 1), CancellationToken.None);

        // Assert – in the cart, gone from "for later"
        Assert.True(ok);
        Assert.Equal(1, (await db.CartItems.SingleAsync()).Quantity);
        Assert.Empty(db.ForLater);
    }

    [Fact]
    public async Task GetCart_ShowsCurrentPriceAfterPriceChange()
    {
        // Arrange – added at 5.00, price later raised to 6.00
        using var db = TestDb.Create();
        var medicine = await TestDb.AddMedicineAsync(db, price: 5m);
        await new AddToCartCommandHandler(db, new FakeCurrentUser(1))
            .Handle(new AddToCartCommand { MedicineId = medicine.Id, Quantity = 2 }, CancellationToken.None);
        medicine.Price = 6m;
        await db.SaveChangesAsync();

        // Act
        var cart = await new GetUserCartQueryHandler(db, new FakeCurrentUser(1))
            .Handle(new GetUserCartQuery(), CancellationToken.None);

        // Assert – 2 x 6.00, the same amount checkout will charge
        Assert.Equal(12m, Assert.Single(cart.Items).Price);
    }
}
