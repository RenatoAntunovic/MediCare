using MediCare.Application.Abstractions;
using MediCare.Application.Common;
using MediCare.Application.Common.Exceptions;
using MediCare.Application.Modules.Medicine.Medicine.Commands.Status.Enable;
using MediCare.Application.Modules.Medicine.Medicine.Queries.List;
using MediCare.Application.Modules.Procurement.Suppliers.Commands.Create;
using MediCare.Application.Modules.Procurement.Suppliers.Queries.List;
using MediCare.Application.Modules.Sales.Orders.Commands.Status;
using MediCare.Application.Modules.Sales.Orders.Queries.MyReport;
using MediCare.Domain.Entities.HospitalRecords;

namespace MediCare.Tests.UnitTests;

/// <summary>
/// Tests for the fixes from the professor's review (catalog, order status, report, suppliers).
/// </summary>
public class ReviewFixesTests
{
    // ---------- helpers ----------

    private sealed class FakeAdmin : IAppCurrentUser
    {
        public int? UserId => 1;
        public string? Email => null;
        public bool IsAuthenticated => true;
        public bool IsAdmin => true;
        public bool IsManager => false;
        public bool IsEmployee => false;
    }

    /// <summary>Counts push notifications instead of sending them.</summary>
    private sealed class FakePush : IPushNotificationService
    {
        public int Sent { get; private set; }

        public Task SendToUserAsync(int userId, string title, string body, CancellationToken ct = default)
        {
            Sent++;
            return Task.CompletedTask;
        }
    }

    private sealed class NoOpIndex : IMedicineSearchIndex
    {
        public Task UpsertAsync(MedicineSearchItemDto item, CancellationToken ct) => Task.CompletedTask;
        public Task RemoveAsync(int medicineId, CancellationToken ct) => Task.CompletedTask;
        public Task RebuildAsync(IReadOnlyList<MedicineSearchItemDto> items, CancellationToken ct) => Task.CompletedTask;
    }

    private static async Task<MedicineCategories> AddCategoryAsync(DatabaseContext db, string name, bool enabled = true)
    {
        var category = new MedicineCategories { Name = name, IsEnabled = enabled };
        db.MedicineCategories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }

    private static async Task<Medicine> AddMedicineAsync(DatabaseContext db, string name, int categoryId, decimal price = 10m, bool enabled = true)
    {
        var medicine = new Medicine
        {
            Name = name,
            Description = "Test",
            ImagePath = "images/test.png",
            Price = price,
            Weight = 100,
            MedicineCategoryId = categoryId,
            isEnabled = enabled
        };
        db.Medicine.Add(medicine);
        await db.SaveChangesAsync();
        return medicine;
    }

    // ---------- #3 / #4 catalog ----------

    [Fact]
    public async Task ListMedicine_NonAdmin_GetsOnlyActive_EvenWhenAskingForDisabled()
    {
        using var db = TestDb.Create();
        var cat = await AddCategoryAsync(db, "Analgetici");
        await AddMedicineAsync(db, "Aktivan", cat.Id);
        await AddMedicineAsync(db, "Neaktivan", cat.Id, enabled: false);

        var handler = new ListMedicineQueryHandler(db, new FakeCurrentUser(5));
        var result = await handler.Handle(new ListMedicineQuery { OnlyEnabled = false }, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("Aktivan", result.Items[0].Name);
    }

    [Fact]
    public async Task ListMedicine_Admin_SeesDisabled_AndFiltersByPriceAndCategory()
    {
        using var db = TestDb.Create();
        var a = await AddCategoryAsync(db, "A");
        var b = await AddCategoryAsync(db, "B");
        await AddMedicineAsync(db, "A1", a.Id, 5m);
        await AddMedicineAsync(db, "A2", a.Id, 15m, enabled: false);
        await AddMedicineAsync(db, "A3", a.Id, 25m);
        await AddMedicineAsync(db, "B1", b.Id, 15m);

        var handler = new ListMedicineQueryHandler(db, new FakeAdmin());
        var result = await handler.Handle(new ListMedicineQuery
        {
            CategoryId = a.Id,
            MinPrice = 10m,
            MaxPrice = 30m,
            Paging = new PageRequest { Page = 1, PageSize = 1 }
        }, CancellationToken.None);

        // A2 (disabled, visible to the admin) and A3 match; the total counts both, the page shows one
        Assert.Equal(2, result.Total);
        Assert.Single(result.Items);
    }

    // ---------- #5 active medicine in a disabled category ----------

    [Fact]
    public async Task EnableMedicine_InDisabledCategory_Throws()
    {
        using var db = TestDb.Create();
        var cat = await AddCategoryAsync(db, "Neaktivna", enabled: false);
        var medicine = await AddMedicineAsync(db, "Lijek", cat.Id, enabled: false);

        var handler = new EnableMedicineCommandHandler(db, new NoOpIndex());

        await Assert.ThrowsAsync<MediCareBusinessRuleException>(() =>
            handler.Handle(new EnableMedicineCommand { Id = medicine.Id }, CancellationToken.None));
        Assert.False((await db.Medicine.FindAsync(medicine.Id))!.isEnabled);
    }

    // ---------- #7 order status transitions ----------

    [Theory]
    [InlineData("DRAFT", "CONFIRMED", true)]
    [InlineData("CONFIRMED", "PAID", true)]
    [InlineData("PAID", "COMPLETED", true)]
    [InlineData("PAID", "CANCELLED", true)]
    [InlineData("DRAFT", "COMPLETED", false)]
    [InlineData("COMPLETED", "CANCELLED", false)]
    [InlineData("CANCELLED", "DRAFT", false)]
    public void OrderStatusTransitions_MatchTheAdminDialog(string from, string to, bool allowed)
    {
        Assert.Equal(allowed, OrderStatusTransitions.IsAllowed(from, to));
    }

    [Fact]
    public async Task ChangeStatus_OfCompletedOrder_IsRejected_AndNoPushIsSent()
    {
        using var db = TestDb.Create();
        var completed = new OrderStatus { StatusName = "COMPLETED" };
        var cancelled = new OrderStatus { StatusName = "CANCELLED" };
        db.OrderStatus.AddRange(completed, cancelled);
        var order = new Orders { UserId = 1, OrderDate = DateTime.UtcNow, TotalPrice = 10m, OrderStatus = completed };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var push = new FakePush();
        var handler = new ChangeOrderStatusCommandHandler(db, push);

        await Assert.ThrowsAsync<MediCareBusinessRuleException>(() =>
            handler.Handle(new ChangeOrderStatusCommand { Id = order.Id, NewStatusId = cancelled.Id }, CancellationToken.None));

        Assert.Equal(0, push.Sent);
        Assert.Equal(completed.Id, (await db.Orders.FindAsync(order.Id))!.OrderStatusId);
    }

    // ---------- #9 report: cancelled orders are not spending ----------

    [Fact]
    public async Task MyOrdersReport_CancelledOrder_IsListedButNotSpent()
    {
        using var db = TestDb.Create();
        var cat = await AddCategoryAsync(db, "A");
        var medicine = await AddMedicineAsync(db, "Brufen", cat.Id, 10m);
        var paid = new OrderStatus { StatusName = "PAID" };
        var cancelled = new OrderStatus { StatusName = "CANCELLED" };
        db.OrderStatus.AddRange(paid, cancelled);
        db.Orders.AddRange(
            new Orders { UserId = 1, OrderDate = DateTime.UtcNow, TotalPrice = 20m, OrderStatus = paid,
                OrderItems = { new OrderItems { MedicineId = medicine.Id, Quantity = 2, Price = 20m } } },
            new Orders { UserId = 1, OrderDate = DateTime.UtcNow, TotalPrice = 50m, OrderStatus = cancelled,
                OrderItems = { new OrderItems { MedicineId = medicine.Id, Quantity = 5, Price = 50m } } });
        await db.SaveChangesAsync();

        var handler = new MyOrdersReportQueryHandler(db, new FakeCurrentUser(1));
        var report = await handler.Handle(new MyOrdersReportQuery(), CancellationToken.None);

        Assert.Equal(2, report.OrdersCount);   // both orders are listed
        Assert.Equal(70m, report.OrdersValue); // value of all orders
        Assert.Equal(20m, report.TotalSpent);  // only the paid one is spending
        Assert.Equal(20m, report.AverageOrder);
        Assert.Equal(2, report.ItemsCount);
    }

    // ---------- suppliers CRUD ----------

    [Fact]
    public async Task CreateSupplier_DuplicateCompanyName_Throws()
    {
        using var db = TestDb.Create();
        var handler = new CreateSupplierCommandHandler(db);
        var command = new CreateSupplierCommand
        {
            CompanyName = "Bosnalijek", ContactName = "Amra", Email = "amra@bl.ba",
            Phone = "033 254-400", Address = "Jukićeva 53", City = "Sarajevo"
        };

        await handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<MediCareConflictException>(() => handler.Handle(new CreateSupplierCommand
        {
            CompanyName = " bosnalijek ", ContactName = "X", Email = "x@bl.ba",
            Phone = "033 111-111", Address = "A", City = "Mostar"
        }, CancellationToken.None));
    }

    [Fact]
    public async Task ListSuppliers_FiltersByCityStatusAndMedicine()
    {
        using var db = TestDb.Create();
        var cat = await AddCategoryAsync(db, "A");
        var brufen = await AddMedicineAsync(db, "Brufen", cat.Id);
        var create = new CreateSupplierCommandHandler(db);

        await create.Handle(new CreateSupplierCommand { CompanyName = "S1", ContactName = "K1", Email = "s1@x.ba",
            Phone = "061 111-111", Address = "A", City = "Mostar", MedicineIds = [brufen.Id] }, CancellationToken.None);
        await create.Handle(new CreateSupplierCommand { CompanyName = "S2", ContactName = "K2", Email = "s2@x.ba",
            Phone = "061 222-222", Address = "A", City = "Mostar" }, CancellationToken.None);
        await create.Handle(new CreateSupplierCommand { CompanyName = "S3", ContactName = "K3", Email = "s3@x.ba",
            Phone = "061 333-333", Address = "A", City = "Sarajevo", MedicineIds = [brufen.Id], IsEnabled = false }, CancellationToken.None);

        var list = new ListSuppliersQueryHandler(db);

        var byCity = await list.Handle(new ListSuppliersQuery { City = "Mostar" }, CancellationToken.None);
        Assert.Equal(2, byCity.Total);

        var byMedicineActive = await list.Handle(new ListSuppliersQuery { MedicineId = brufen.Id, OnlyEnabled = true }, CancellationToken.None);
        Assert.Single(byMedicineActive.Items);
        Assert.Equal("S1", byMedicineActive.Items[0].CompanyName);
    }

    [Fact]
    public void SupplierValidator_RejectsInvalidEmailAndPhone()
    {
        var validator = new CreateSupplierCommandValidator();
        var result = validator.Validate(new CreateSupplierCommand
        {
            CompanyName = "Firma", ContactName = "Osoba", Email = "nije-email",
            Phone = "abc", Address = "Adresa", City = "Grad"
        });

        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
        Assert.Contains(result.Errors, e => e.PropertyName == "Phone");
    }
}
