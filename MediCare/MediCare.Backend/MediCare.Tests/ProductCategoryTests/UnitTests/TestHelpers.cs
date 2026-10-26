using MediCare.Application.Abstractions;
using MediCare.Domain.Entities.HospitalRecords;
using Microsoft.Extensions.Time.Testing;

namespace MediCare.Tests.UnitTests;

/// <summary>
/// Creates a fresh in-memory database for every test, so tests never affect each other
/// and don't need SQL Server.
/// </summary>
internal static class TestDb
{
    public static DatabaseContext Create()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DatabaseContext(options, new FakeTimeProvider());
    }

    public static async Task<Medicine> AddMedicineAsync(DatabaseContext db, decimal price, bool isEnabled = true)
    {
        var medicine = new Medicine
        {
            Name = "Brufen",
            Description = "Test medicine",
            ImagePath = "images/test.png",
            Price = price,
            Weight = 200,
            MedicineCategoryId = 1,
            isEnabled = isEnabled
        };

        db.Medicine.Add(medicine);
        await db.SaveChangesAsync();
        return medicine;
    }
}

/// <summary>
/// Fake logged-in user (replaces the user that normally comes from the JWT).
/// </summary>
internal sealed class FakeCurrentUser(int? userId) : IAppCurrentUser
{
    public int? UserId { get; } = userId;
    public string? Email => null;
    public bool IsAuthenticated => UserId != null;
    public bool IsAdmin => false;
    public bool IsManager => false;
    public bool IsEmployee => true;
}