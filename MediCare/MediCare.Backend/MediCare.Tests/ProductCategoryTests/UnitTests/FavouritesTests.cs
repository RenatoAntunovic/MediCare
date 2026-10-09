using MediCare.Application.Modules.Favourites.Command.Create;

namespace MediCare.Tests.UnitTests;

public class FavouritesTests
{
    [Fact]
    public async Task AddToFavourites_SameMedicineTwice_ReturnsExistingIdWithoutDuplicate()
    {
        // Arrange
        using var db = TestDb.Create();
        var medicine = await TestDb.AddMedicineAsync(db, price: 4.00m);
        var handler = new AddToFavouritesCommandHandler(db, new FakeCurrentUser(1));
        var command = new AddToFavouritesCommand { MedicineId = medicine.Id };

        // Act
        var firstId = await handler.Handle(command, CancellationToken.None);
        var secondId = await handler.Handle(command, CancellationToken.None);

        // Assert – same favourite returned, only one row in the database
        Assert.Equal(firstId, secondId);
        Assert.Single(db.Favourites);
    }

    [Fact]
    public async Task AddToFavourites_AnonymousUser_ThrowsUnauthorized()
    {
        // Arrange
        using var db = TestDb.Create();
        var handler = new AddToFavouritesCommandHandler(db, new FakeCurrentUser(null));

        // Act + Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new AddToFavouritesCommand { MedicineId = 1 }, CancellationToken.None));
    }
}