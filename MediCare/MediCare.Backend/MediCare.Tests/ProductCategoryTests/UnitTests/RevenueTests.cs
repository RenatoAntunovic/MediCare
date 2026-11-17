using MediCare.Application.Modules.Sales.Orders.Queries.Report;
using MediCare.Application.Modules.Statistics.Queries.Dashboard;
using MediCare.Domain.Entities.HospitalRecords;

namespace MediCare.Tests.UnitTests;

/// <summary>Realised revenue = PAID (and COMPLETED) only; CONFIRMED is shown apart; DRAFT and CANCELLED are not money.</summary>
public class RevenueTests
{
    private static async Task<Dictionary<string, OrderStatus>> AddAllStatusesAsync(DatabaseContext db)
    {
        var statuses = new[] { "DRAFT", "CONFIRMED", "PAID", "COMPLETED", "CANCELLED" }
            .Select(n => new OrderStatus { StatusName = n }).ToList();
        db.OrderStatus.AddRange(statuses);
        await db.SaveChangesAsync();
        return statuses.ToDictionary(s => s.StatusName);
    }

    private static Orders Order(OrderStatus status, decimal total, DateTime? date = null) =>
        new() { UserId = 1, OrderDate = date ?? new DateTime(2026, 3, 10), TotalPrice = total, OrderStatusId = status.Id };

    [Fact]
    public async Task Dashboard_PaidConfirmedAndCancelled_OnlyPaidIsRevenue_ConfirmedIsShownSeparately()
    {
        // Arrange – same period: paid 20, confirmed 30, cancelled 50
        using var db = TestDb.Create();
        var s = await AddAllStatusesAsync(db);
        db.Orders.AddRange(Order(s["PAID"], 20m), Order(s["CONFIRMED"], 30m), Order(s["CANCELLED"], 50m));
        await db.SaveChangesAsync();
        var handler = new StatisticsDashboardQueryHandler(db);

        // Act
        var result = await handler.Handle(new StatisticsDashboardQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(20m, result.OrdersRevenue);
        Assert.Equal(30m, result.OrdersConfirmedAmount);
        Assert.Equal(3, result.OrdersCount);
        // the monthly chart uses the same criterion as the total
        Assert.Equal(20m, Assert.Single(result.ByMonth).OrdersRevenue);
    }

    [Fact]
    public async Task Dashboard_DraftIsNotRevenueAndNotConfirmed_CompletedIsRevenue()
    {
        using var db = TestDb.Create();
        var s = await AddAllStatusesAsync(db);
        db.Orders.AddRange(Order(s["DRAFT"], 40m), Order(s["COMPLETED"], 15m), Order(s["PAID"], 5m));
        await db.SaveChangesAsync();
        var handler = new StatisticsDashboardQueryHandler(db);

        var result = await handler.Handle(new StatisticsDashboardQuery(), CancellationToken.None);

        Assert.Equal(20m, result.OrdersRevenue);        // completed 15 + paid 5
        Assert.Equal(0m, result.OrdersConfirmedAmount); // a draft is not "confirmed"
    }

    [Fact]
    public async Task Dashboard_Reservations_UseTheSameRule()
    {
        using var db = TestDb.Create();
        var s = await AddAllStatusesAsync(db);
        var treatment = new Treatments { ServiceName = "Pregled", Description = "x", ImagePath = "x", Price = 15m, TreatmentCategoryId = 1, isEnabled = true };
        db.Treatments.Add(treatment);
        await db.SaveChangesAsync();

        Reservations Reservation(OrderStatus status, decimal price, int hour) => new()
        {
            UserId = 1, TreatmentId = treatment.Id, ReservationDate = new DateTime(2026, 3, 10),
            ReservationTime = new TimeSpan(hour, 0, 0), OrderStatusId = status.Id, Price = price
        };
        db.Reservations.AddRange(
            Reservation(s["PAID"], 20m, 9), Reservation(s["CONFIRMED"], 30m, 10), Reservation(s["CANCELLED"], 50m, 11));
        await db.SaveChangesAsync();
        var handler = new StatisticsDashboardQueryHandler(db);

        var result = await handler.Handle(new StatisticsDashboardQuery(), CancellationToken.None);

        Assert.Equal(20m, result.ReservationsRevenue);
        Assert.Equal(30m, result.ReservationsConfirmedAmount);
        Assert.Equal(20m, Assert.Single(result.ByMonth).ReservationsRevenue);
        Assert.Equal(3, result.ReservationsCount);
    }

    [Fact]
    public async Task Dashboard_TopMedicines_AreCountedFromPaidOrdersOnly()
    {
        using var db = TestDb.Create();
        var s = await AddAllStatusesAsync(db);
        var medicine = await TestDb.AddMedicineAsync(db, price: 5m);
        var paid = Order(s["PAID"], 10m);
        var confirmed = Order(s["CONFIRMED"], 50m);
        db.Orders.AddRange(paid, confirmed);
        await db.SaveChangesAsync();
        db.OrderItems.AddRange(
            new OrderItems { OrderId = paid.Id, MedicineId = medicine.Id, Quantity = 2, Price = 10m },
            new OrderItems { OrderId = confirmed.Id, MedicineId = medicine.Id, Quantity = 10, Price = 50m });
        await db.SaveChangesAsync();
        var handler = new StatisticsDashboardQueryHandler(db);

        var result = await handler.Handle(new StatisticsDashboardQuery(), CancellationToken.None);

        var top = Assert.Single(result.TopMedicines);
        Assert.Equal(2, top.Quantity);
        Assert.Equal(10m, top.Revenue);
    }

    [Fact]
    public async Task OrdersReport_TotalRevenueIsPaidOnly_ConfirmedIsShownSeparately()
    {
        // The admin PDF report ("Ukupan promet") must follow the same rule as the dashboard
        using var db = TestDb.Create();
        var s = await AddAllStatusesAsync(db);
        db.Users.Add(new Users { FirstName = "Amra", LastName = "H", City = "Mostar", Adress = "x", UserName = "a", Email = "a@a.com", PhoneNumber = "1", PasswordHash = "x" });
        await db.SaveChangesAsync();
        var userId = (await db.Users.SingleAsync()).Id;
        db.Orders.AddRange(
            new Orders { UserId = userId, OrderDate = new DateTime(2026, 3, 10), TotalPrice = 20m, OrderStatusId = s["PAID"].Id },
            new Orders { UserId = userId, OrderDate = new DateTime(2026, 3, 11), TotalPrice = 30m, OrderStatusId = s["CONFIRMED"].Id },
            new Orders { UserId = userId, OrderDate = new DateTime(2026, 3, 12), TotalPrice = 50m, OrderStatusId = s["CANCELLED"].Id });
        await db.SaveChangesAsync();
        var handler = new OrdersReportQueryHandler(db);

        var report = await handler.Handle(new OrdersReportQuery(), CancellationToken.None);

        Assert.Equal(3, report.OrdersCount);
        Assert.Equal(20m, report.TotalRevenue);
        Assert.Equal(30m, report.ConfirmedNotPaid);
    }
}
