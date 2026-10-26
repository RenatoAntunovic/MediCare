using MediCare.Domain.Entities.HospitalRecords;
using MediCare.Application.Modules.Statistics.Queries.Dashboard;

namespace MediCare.Tests.UnitTests;

public class StatisticsTests
{
    private static async Task<(OrderStatus Paid, OrderStatus Cancelled)> AddStatusesAsync(DatabaseContext db)
    {
        var paid = new OrderStatus { StatusName = "PAID" };
        var cancelled = new OrderStatus { StatusName = "CANCELLED" };
        db.OrderStatus.AddRange(paid, cancelled);
        await db.SaveChangesAsync();
        return (paid, cancelled);
    }

    [Fact]
    public async Task Dashboard_CancelledOrders_AreCountedButNotInRevenue()
    {
        // Arrange
        using var db = TestDb.Create();
        var (paid, cancelled) = await AddStatusesAsync(db);
        db.Orders.AddRange(
            new Orders { UserId = 1, OrderDate = new DateTime(2026, 3, 10), TotalPrice = 20m, OrderStatusId = paid.Id },
            new Orders { UserId = 1, OrderDate = new DateTime(2026, 3, 12), TotalPrice = 50m, OrderStatusId = cancelled.Id });
        await db.SaveChangesAsync();
        var handler = new StatisticsDashboardQueryHandler(db);

        // Act
        var result = await handler.Handle(new StatisticsDashboardQuery(), CancellationToken.None);

        // Assert – both orders counted, only the paid one is revenue
        Assert.Equal(2, result.OrdersCount);
        Assert.Equal(20m, result.OrdersRevenue);
        Assert.Equal(2, result.OrdersByStatus.Count);
    }

    [Fact]
    public async Task Dashboard_MonthsWithoutData_AreIncludedWithZero()
    {
        // Arrange
        using var db = TestDb.Create();
        var (paid, _) = await AddStatusesAsync(db);
        db.Orders.AddRange(
            new Orders { UserId = 1, OrderDate = new DateTime(2026, 1, 5), TotalPrice = 10m, OrderStatusId = paid.Id },
            new Orders { UserId = 1, OrderDate = new DateTime(2026, 3, 5), TotalPrice = 30m, OrderStatusId = paid.Id });
        await db.SaveChangesAsync();
        var handler = new StatisticsDashboardQueryHandler(db);

        // Act
        var result = await handler.Handle(new StatisticsDashboardQuery(), CancellationToken.None);

        // Assert – January, February (empty), March
        Assert.Equal([1, 2, 3], result.ByMonth.Select(m => m.Month));
        Assert.Equal([10m, 0m, 30m], result.ByMonth.Select(m => m.OrdersRevenue));
    }

    [Fact]
    public async Task Dashboard_Period_FiltersOrdersAndReservations()
    {
        // Arrange
        using var db = TestDb.Create();
        var (paid, _) = await AddStatusesAsync(db);
        var treatment = new Treatments { ServiceName = "Pregled", Description = "x", ImagePath = "x", Price = 15m, TreatmentCategoryId = 1, isEnabled = true };
        db.Treatments.Add(treatment);
        await db.SaveChangesAsync();

        db.Orders.AddRange(
            new Orders { UserId = 1, OrderDate = new DateTime(2026, 2, 28), TotalPrice = 10m, OrderStatusId = paid.Id },
            new Orders { UserId = 1, OrderDate = new DateTime(2026, 3, 31, 18, 0, 0), TotalPrice = 30m, OrderStatusId = paid.Id });
        db.Reservations.AddRange(
            new Reservations { UserId = 1, TreatmentId = treatment.Id, ReservationDate = new DateTime(2026, 3, 2), ReservationTime = new TimeSpan(9, 0, 0), OrderStatusId = paid.Id, Price = 15m },
            new Reservations { UserId = 1, TreatmentId = treatment.Id, ReservationDate = new DateTime(2026, 4, 1), ReservationTime = new TimeSpan(9, 0, 0), OrderStatusId = paid.Id, Price = 15m });
        await db.SaveChangesAsync();
        var handler = new StatisticsDashboardQueryHandler(db);

        // Act – March only; "to" includes the whole last day
        var result = await handler.Handle(
            new StatisticsDashboardQuery { From = new DateTime(2026, 3, 1), To = new DateTime(2026, 3, 31) },
            CancellationToken.None);

        // Assert
        Assert.Equal(1, result.OrdersCount);
        Assert.Equal(30m, result.OrdersRevenue);
        Assert.Equal(1, result.ReservationsCount);
        Assert.Equal("Pregled", Assert.Single(result.ReservationsByTreatment).Label);
        Assert.Equal(1, result.ReservationsByWeekday.Single(d => d.Label == "Monday").Count);
    }

    [Fact]
    public async Task Dashboard_LowStock_UsesThreshold()
    {
        // Arrange
        using var db = TestDb.Create();
        var medicine = await TestDb.AddMedicineAsync(db, price: 5m);
        db.Inventories.Add(new Inventories { MedicineId = medicine.Id, QuantityInStock = 4 });
        await db.SaveChangesAsync();
        var handler = new StatisticsDashboardQueryHandler(db);

        // Act
        var low = await handler.Handle(new StatisticsDashboardQuery { LowStockThreshold = 5 }, CancellationToken.None);
        var notLow = await handler.Handle(new StatisticsDashboardQuery { LowStockThreshold = 3 }, CancellationToken.None);

        // Assert
        Assert.Equal(1, low.LowStockCount);
        Assert.Equal(0, notLow.LowStockCount);
    }
}
