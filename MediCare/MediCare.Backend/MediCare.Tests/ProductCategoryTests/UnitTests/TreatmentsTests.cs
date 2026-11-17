using MediCare.Application.Abstractions;
using MediCare.Application.Common;
using MediCare.Application.Common.Exceptions;
using MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Status.Disable;
using MediCare.Application.Modules.Catalog.TreatmentCategories.Queries.List;
using MediCare.Application.Modules.Catalog.Treatments.Commands.Update;
using MediCare.Application.Modules.Catalog.Treatments.Queries.List;
using MediCare.Domain.Entities.HospitalRecords;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace MediCare.Tests.UnitTests;

public class TreatmentsTests
{
    // ---------- helpers ----------

    private static async Task<TreatmentCategories> AddCategoryAsync(DatabaseContext db, string name = "General", bool enabled = true)
    {
        var category = new TreatmentCategories { CategoryName = name, isEnabled = enabled };
        db.TreatmentCategories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }

    private static async Task<Treatments> AddTreatmentAsync(
        DatabaseContext db, int categoryId, string name, string imagePath = "images/old.png")
    {
        var treatment = new Treatments
        {
            ServiceName = name,
            Description = "Test treatment",
            Price = 50m,
            ImagePath = imagePath,
            TreatmentCategoryId = categoryId,
            isEnabled = true
        };
        db.Treatments.Add(treatment);
        await db.SaveChangesAsync();
        return treatment;
    }

    private static IFormFile FakeImage(string fileName = "new.png")
    {
        var stream = new MemoryStream([1, 2, 3]);
        return new FormFile(stream, 0, stream.Length, "ImageFile", fileName);
    }

    private static UpdateTreatmentsCommand UpdateWithImage(Treatments t) => new()
    {
        Id = t.Id,
        ServiceName = t.ServiceName,
        Description = t.Description,
        Price = t.Price,
        TreatmentCategoryId = t.TreatmentCategoryId,
        ImageFile = FakeImage()
    };

    // ---------- fix 5: total comes from the FILTERED set ----------

    [Fact]
    public async Task ListTreatments_Total_IsCountOfFilteredSet_NotWholeTable()
    {
        // Arrange – 10 treatments in the table, 3 of them match "Massage"
        using var db = TestDb.Create();
        var category = await AddCategoryAsync(db);
        for (var i = 1; i <= 3; i++) await AddTreatmentAsync(db, category.Id, $"Massage {i}");
        for (var i = 1; i <= 7; i++) await AddTreatmentAsync(db, category.Id, $"Checkup {i}");
        var handler = new ListTreatmentsQueryHandler(db);

        // Act – page size 2 so the page holds fewer items than the total
        var result = await handler.Handle(
            new ListTreatmentsQuery { Search = "Massage", Paging = new PageRequest { Page = 1, PageSize = 2 } },
            CancellationToken.None);

        // Assert – total is 3 (filtered), not 10 (table), and the page has 2 items
        Assert.Equal(3, result.Total);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task ListTreatments_SecondPage_ReturnsRemainingItemsWithSameTotal()
    {
        using var db = TestDb.Create();
        var category = await AddCategoryAsync(db);
        for (var i = 1; i <= 3; i++) await AddTreatmentAsync(db, category.Id, $"Massage {i}");
        await AddTreatmentAsync(db, category.Id, "Checkup");
        var handler = new ListTreatmentsQueryHandler(db);

        var result = await handler.Handle(
            new ListTreatmentsQuery { Search = "Massage", Paging = new PageRequest { Page = 2, PageSize = 2 } },
            CancellationToken.None);

        Assert.Equal(3, result.Total);
        Assert.Single(result.Items);
    }

    // ---------- treatment categories: Search + OnlyEnabled both work on the server ----------

    [Fact]
    public async Task ListTreatmentCategories_SearchAndOnlyEnabled_FilterOnServer_AndTotalMatchesFilter()
    {
        // Arrange
        using var db = TestDb.Create();
        await AddCategoryAsync(db, "Surgery A", enabled: true);
        await AddCategoryAsync(db, "Surgery B", enabled: true);
        await AddCategoryAsync(db, "Surgery C", enabled: false);
        await AddCategoryAsync(db, "Dental", enabled: true);
        var handler = new ListTreatmentCategoriesQueryHandler(db);

        // Act
        var result = await handler.Handle(
            new ListTreatmentCategoriesQuery
            {
                Search = "Surgery",
                OnlyEnabled = true,
                Paging = new PageRequest { Page = 1, PageSize = 1 }
            },
            CancellationToken.None);

        // Assert – 2 enabled "Surgery" categories, one on this page
        Assert.Equal(2, result.Total);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task ListTreatmentCategories_OnlyEnabledNull_ReturnsAllCategories()
    {
        // The frontend sends nothing (null) when the "only enabled" toggle is off
        using var db = TestDb.Create();
        await AddCategoryAsync(db, "A", enabled: true);
        await AddCategoryAsync(db, "B", enabled: false);
        var handler = new ListTreatmentCategoriesQueryHandler(db);

        var result = await handler.Handle(new ListTreatmentCategoriesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Total);
    }

    [Fact]
    public async Task DisableCategory_WithActiveTreatment_IsRejected_AndWithoutOneIsDisabled()
    {
        using var db = TestDb.Create();
        var busy = await AddCategoryAsync(db, "Busy");
        var free = await AddCategoryAsync(db, "Free");
        await AddTreatmentAsync(db, busy.Id, "Massage");
        var handler = new DisableTreatmentCategoryCommandHandler(db);

        // business rule -> the API answers 409 Conflict
        await Assert.ThrowsAsync<MediCareBusinessRuleException>(() =>
            handler.Handle(new DisableTreatmentCategoryCommand { Id = busy.Id }, CancellationToken.None));
        Assert.True((await db.TreatmentCategories.FindAsync(busy.Id))!.isEnabled);

        await handler.Handle(new DisableTreatmentCategoryCommand { Id = free.Id }, CancellationToken.None);
        Assert.False((await db.TreatmentCategories.FindAsync(free.Id))!.isEnabled);
    }

    // ---------- fix 6: update treatment image – order of operations ----------

    [Fact]
    public async Task UpdateTreatment_StorageFailsOnCopy_KeepsOldImageInDbAndOnDisk()
    {
        // Arrange
        using var db = TestDb.Create();
        var category = await AddCategoryAsync(db);
        var treatment = await AddTreatmentAsync(db, category.Id, "Massage");
        var storage = new FakeImageStorage { FailOnSave = true };
        var handler = new UpdateTreatmentsCommandHandler(db, storage, NullLogger<UpdateTreatmentsCommandHandler>.Instance);

        // Act
        await Assert.ThrowsAsync<IOException>(() =>
            handler.Handle(UpdateWithImage(treatment), CancellationToken.None));

        // Assert – the old image is still linked and was never deleted
        Assert.Equal("images/old.png", (await db.Treatments.AsNoTracking().SingleAsync()).ImagePath);
        Assert.Empty(storage.Deleted);
    }

    [Fact]
    public async Task UpdateTreatment_SaveChangesFails_KeepsOldImageAndDeletesNewFile()
    {
        // Arrange – a context whose SaveChanges throws after the seed data is saved
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new FailingSaveDbContext(options);
        var category = await AddCategoryAsync(db);
        var treatment = await AddTreatmentAsync(db, category.Id, "Massage");
        db.FailSaves = true;
        var storage = new FakeImageStorage();
        var handler = new UpdateTreatmentsCommandHandler(db, storage, NullLogger<UpdateTreatmentsCommandHandler>.Instance);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(UpdateWithImage(treatment), CancellationToken.None));

        // Assert – only the NEW file was removed, the old one was not touched
        Assert.Single(storage.Saved);
        Assert.Equal(storage.Saved, storage.Deleted);
        Assert.DoesNotContain("images/old.png", storage.Deleted);
    }

    [Fact]
    public async Task UpdateTreatment_Success_SavesNewPathAndRemovesOldFile()
    {
        using var db = TestDb.Create();
        var category = await AddCategoryAsync(db);
        var treatment = await AddTreatmentAsync(db, category.Id, "Massage");
        var storage = new FakeImageStorage();
        var handler = new UpdateTreatmentsCommandHandler(db, storage, NullLogger<UpdateTreatmentsCommandHandler>.Instance);

        await handler.Handle(UpdateWithImage(treatment), CancellationToken.None);

        var saved = await db.Treatments.AsNoTracking().SingleAsync();
        Assert.Equal(storage.Saved.Single(), saved.ImagePath);
        Assert.Equal(["images/old.png"], storage.Deleted); // old file removed after the DB save
    }

    [Fact]
    public async Task UpdateTreatment_OldImageUsedByAnotherTreatment_KeepsOldFile()
    {
        using var db = TestDb.Create();
        var category = await AddCategoryAsync(db);
        var treatment = await AddTreatmentAsync(db, category.Id, "Massage", "images/shared.png");
        await AddTreatmentAsync(db, category.Id, "Checkup", "images/shared.png");
        var storage = new FakeImageStorage();
        var handler = new UpdateTreatmentsCommandHandler(db, storage, NullLogger<UpdateTreatmentsCommandHandler>.Instance);

        await handler.Handle(UpdateWithImage(treatment), CancellationToken.None);

        Assert.Empty(storage.Deleted); // the other treatment still uses the file
    }

    [Fact]
    public async Task UpdateTreatment_RemovingOldFileFails_RequestStillSucceeds()
    {
        using var db = TestDb.Create();
        var category = await AddCategoryAsync(db);
        var treatment = await AddTreatmentAsync(db, category.Id, "Massage");
        var storage = new FakeImageStorage { FailOnDelete = true };
        var handler = new UpdateTreatmentsCommandHandler(db, storage, NullLogger<UpdateTreatmentsCommandHandler>.Instance);

        await handler.Handle(UpdateWithImage(treatment), CancellationToken.None); // must not throw

        Assert.Equal(storage.Saved.Single(), (await db.Treatments.AsNoTracking().SingleAsync()).ImagePath);
    }

    // ---------- fakes ----------

    /// <summary>In-memory storage that records saved/deleted paths and can fail on demand.</summary>
    private sealed class FakeImageStorage : IImageStorage
    {
        public bool FailOnSave { get; init; }
        public bool FailOnDelete { get; init; }
        public List<string> Saved { get; } = [];
        public List<string> Deleted { get; } = [];

        public Task<string> SaveAsync(IFormFile file, CancellationToken ct)
        {
            if (FailOnSave) throw new IOException("Copy failed.");
            var path = $"images/{Guid.NewGuid()}_{file.FileName}";
            Saved.Add(path);
            return Task.FromResult(path);
        }

        public void Delete(string relativePath)
        {
            if (FailOnDelete) throw new IOException("Delete failed.");
            Deleted.Add(relativePath);
        }
    }

    /// <summary>Real in-memory context, but SaveChangesAsync can be switched to throw.</summary>
    private sealed class FailingSaveDbContext(DbContextOptions<DatabaseContext> options)
        : DatabaseContext(options, new FakeTimeProvider())
    {
        public bool FailSaves { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => FailSaves
                ? throw new InvalidOperationException("Database save failed.")
                : base.SaveChangesAsync(cancellationToken);
    }
}
