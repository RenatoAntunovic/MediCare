using FluentValidation;
using MediatR;
using MediCare.Application.Abstractions;
using MediCare.Application.Common.Behaviors;
using MediCare.Application.Common.Exceptions;
using MediCare.Application.Modules.Reservations.CreateReservation.Commands.Create;
using MediCare.Application.Modules.Reservations.Queries.TakenSlots;
using MediCare.Domain.Entities.HospitalRecords;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace MediCare.Tests.IntegrationTests;

/// <summary>
/// Whole booking flow without the browser: request -> FluentValidation (ValidationBehavior) -> handler ->
/// EF Core -> a REAL SQL Server database with the real model (unique index IX_Reservations_Slot).
/// Each test gets its own temporary LocalDB database, dropped at the end.
/// The tests are skipped (not failed) on a machine without SQL Server LocalDB.
/// </summary>
public class ReservationFlowTests : IAsyncLifetime
{
    private readonly string _connectionString =
        $"Server=(localdb)\\MSSQLLocalDB;Database=MediCareTest_{Guid.NewGuid():N};Trusted_Connection=True;TrustServerCertificate=True";

    private ServiceProvider _services = null!;
    private IServiceScope _scope = null!;
    private DatabaseContext _db = null!;
    private ISender _sender = null!;
    private int _treatmentId;
    private int _userId;
    private int _otherUserId;

    // ---------- setup / cleanup ----------

    public async Task InitializeAsync()
    {
        if (!LocalDbFactAttribute.IsLocalDbAvailable()) return; // the tests are skipped anyway

        _services = new ServiceCollection()
            .AddSingleton<TimeProvider>(TimeProvider.System)
            .AddDbContext<DatabaseContext>(o => o.UseSqlServer(_connectionString))
            .AddScoped<IAppDbContext>(sp => sp.GetRequiredService<DatabaseContext>())
            .AddMediatR(c => c.RegisterServicesFromAssembly(typeof(CreateReservationCommand).Assembly))
            .AddValidatorsFromAssembly(typeof(CreateReservationCommand).Assembly)
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
            .BuildServiceProvider();

        _scope = _services.CreateScope();
        _db = _scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        _sender = _scope.ServiceProvider.GetRequiredService<ISender>();

        await _db.Database.EnsureCreatedAsync();
        await SeedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_db is null) return;
        await _db.Database.EnsureDeletedAsync();
        _scope.Dispose();
        await _services.DisposeAsync();
    }

    /// <summary>Ids matter: the unique index ignores OrderStatusId 5 (= CANCELLED), like in the real seed.</summary>
    private async Task SeedAsync()
    {
        _db.OrderStatus.AddRange(
            new OrderStatus { StatusName = "DRAFT" },      // 1
            new OrderStatus { StatusName = "CONFIRMED" },  // 2
            new OrderStatus { StatusName = "PAID" },       // 3
            new OrderStatus { StatusName = "COMPLETED" },  // 4
            new OrderStatus { StatusName = "CANCELLED" }); // 5
        var role = new Roles { Name = "User" };
        var category = new TreatmentCategories { CategoryName = "Pregled", isEnabled = true };
        _db.AddRange(role, category);
        await _db.SaveChangesAsync();

        Users NewUser(string name) => new()
        {
            FirstName = name, LastName = "Test", UserName = name, Email = $"{name}@test.com", PhoneNumber = "061123456",
            City = "Mostar", Adress = "Titova 1", PasswordHash = "x", RoleId = role.Id, IsEnabled = true
        };
        var user = NewUser("amra");
        var other = NewUser("marko");
        var treatment = new Treatments
        {
            ServiceName = "Ortopedija", Description = "Pregled", ImagePath = "x", Price = 70m,
            TreatmentCategoryId = category.Id, isEnabled = true
        };
        _db.AddRange(user, other, treatment);
        await _db.SaveChangesAsync();

        _userId = user.Id;
        _otherUserId = other.Id;
        _treatmentId = treatment.Id;
    }

    // ---------- helpers ----------

    /// <summary>The next working day that is at least 3 days away.</summary>
    private static DateTime BookableDay()
    {
        var day = DateTime.Today.AddDays(3);
        while (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) day = day.AddDays(1);
        return day;
    }

    private CreateReservationCommand Booking(DateTime date, TimeSpan time, int? userId = null, string? notes = null) => new()
    {
        UserId = userId ?? _userId,
        TreatmentId = _treatmentId,
        ReservationDate = date,
        ReservationTime = time,
        Notes = notes
    };

    // ---------- valid reservation ----------

    [LocalDbFact]
    public async Task ValidReservation_IsSavedAsDraft_WithThePriceOfTheTreatment_AndTheSlotShowsAsTaken()
    {
        var day = BookableDay();

        var id = await _sender.Send(Booking(day, new TimeSpan(9, 30, 0), notes: "  Bol u koljenu  "));

        var saved = await _db.Reservations.AsNoTracking().Include(r => r.OrderStatus).SingleAsync(r => r.Id == id);
        Assert.Equal("DRAFT", saved.OrderStatus.StatusName);
        Assert.Equal(70m, saved.Price);                      // the price comes from the treatment, not from the client
        Assert.Equal(day, saved.ReservationDate);
        Assert.Equal(new TimeSpan(9, 30, 0), saved.ReservationTime);
        Assert.Equal("Bol u koljenu", saved.Notes);          // trimmed
        Assert.Equal(_userId, saved.UserId);

        // the calendar greys the slot out afterwards
        var taken = await _sender.Send(new GetTakenSlotsQuery { TreatmentId = _treatmentId, Date = day });
        Assert.Equal(["09:30"], taken);
    }

    // ---------- rejected invalid slot ----------

    [LocalDbTheory]
    [InlineData(18, 0, false)]  // after 17:00
    [InlineData(7, 30, false)]  // before 08:00
    [InlineData(9, 15, false)]  // not a full or half hour
    [InlineData(10, 0, true)]   // a Saturday or Sunday (see the test body)
    public async Task InvalidSlot_IsRejectedByTheServer_AndNothingIsSaved(int hour, int minute, bool weekend)
    {
        var date = BookableDay();
        if (weekend)
            while (date.DayOfWeek != DayOfWeek.Saturday) date = date.AddDays(1);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _sender.Send(Booking(date, new TimeSpan(hour, minute, 0))));

        Assert.Equal(0, await _db.Reservations.CountAsync());
    }

    [LocalDbFact]
    public async Task TodayAndPastDates_AreRejected_BecauseTheEarliestDayIsTomorrow()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _sender.Send(Booking(DateTime.Today, new TimeSpan(9, 0, 0))));
        await Assert.ThrowsAsync<ValidationException>(() => _sender.Send(Booking(DateTime.Today.AddDays(-2), new TimeSpan(9, 0, 0))));
        Assert.Equal(0, await _db.Reservations.CountAsync());
    }

    // ---------- rejected double booking ----------

    [LocalDbFact]
    public async Task DoubleBooking_SecondPatientGetsAConflict_AndOnlyOneReservationExists()
    {
        var day = BookableDay();
        await _sender.Send(Booking(day, new TimeSpan(11, 0, 0)));

        await Assert.ThrowsAsync<MediCareConflictException>(() =>
            _sender.Send(Booking(day, new TimeSpan(11, 0, 0), userId: _otherUserId)));

        Assert.Equal(1, await _db.Reservations.CountAsync());
    }

    [LocalDbFact]
    public async Task UniqueIndex_RejectsADuplicateInTheDatabase_EvenWhenTheApplicationCheckIsBypassed()
    {
        // Two writes at the same moment both pass the "is it taken?" check; the database is the last line of defence.
        var day = BookableDay();
        Reservations Row(int userId) => new()
        {
            UserId = userId, TreatmentId = _treatmentId, ReservationDate = day,
            ReservationTime = new TimeSpan(13, 0, 0), OrderStatusId = 1, Price = 70m
        };
        _db.Reservations.Add(Row(_userId));
        await _db.SaveChangesAsync();

        _db.Reservations.Add(Row(_otherUserId));
        var error = await Assert.ThrowsAsync<DbUpdateException>(() => _db.SaveChangesAsync());

        Assert.IsType<SqlException>(error.InnerException);
        Assert.Contains("IX_Reservations_Slot", error.InnerException!.Message);
    }

    // ---------- the same slot after a cancellation ----------

    [LocalDbFact]
    public async Task CancelledReservation_FreesTheSlotForAnotherPatient()
    {
        var day = BookableDay();
        var firstId = await _sender.Send(Booking(day, new TimeSpan(14, 30, 0)));

        var first = await _db.Reservations.SingleAsync(r => r.Id == firstId);
        first.OrderStatusId = 5; // CANCELLED
        await _db.SaveChangesAsync();

        // calendar: the slot is free again, and a new booking is accepted by the validator, handler and unique index
        Assert.Empty(await _sender.Send(new GetTakenSlotsQuery { TreatmentId = _treatmentId, Date = day }));
        var secondId = await _sender.Send(Booking(day, new TimeSpan(14, 30, 0), userId: _otherUserId));

        Assert.NotEqual(firstId, secondId);
        Assert.Equal(2, await _db.Reservations.CountAsync());
    }
}

/// <summary>[Fact] that is skipped when SQL Server LocalDB is not installed.</summary>
public sealed class LocalDbFactAttribute : FactAttribute
{
    private static bool? _available;

    public LocalDbFactAttribute()
    {
        if (!IsLocalDbAvailable()) Skip = "SQL Server LocalDB is not available on this machine.";
    }

    public static bool IsLocalDbAvailable()
    {
        if (_available is not null) return _available.Value;
        try
        {
            using var connection = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=5");
            connection.Open();
            _available = true;
        }
        catch
        {
            _available = false;
        }
        return _available.Value;
    }
}

/// <summary>[Theory] that is skipped when SQL Server LocalDB is not installed.</summary>
public sealed class LocalDbTheoryAttribute : TheoryAttribute
{
    public LocalDbTheoryAttribute()
    {
        if (!LocalDbFactAttribute.IsLocalDbAvailable()) Skip = "SQL Server LocalDB is not available on this machine.";
    }
}
