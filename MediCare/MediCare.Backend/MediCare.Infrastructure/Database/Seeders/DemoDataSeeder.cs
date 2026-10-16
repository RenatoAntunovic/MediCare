using MediCare.Domain.Entities.HospitalRecords;

namespace MediCare.Infrastructure.Database.Seeders;

/// <summary>
/// Demo data for presentations and the statistics page: patients, orders and reservations
/// spread over the last 12 months (and a few upcoming appointments).
///
/// Runs automatically on startup in Development, right after DynamicDataSeeder (see DatabaseInitializer).
///
/// - Safe on every start: the bulk data is added only if the demo patients don't exist yet.
/// - Dates are relative to the day it first runs, so the charts show the last year.
/// - Random values use a fixed seed, so every machine gets the same kind of data.
/// - Requires the base data from DynamicDataSeeder (roles, statuses, categories).
/// </summary>
public static class DemoDataSeeder
{
    public const string DemoAdminEmail = "admin@medicare.com";
    private const string DemoAdminPassword = "admin123!";
    private const string DemoPatientDomain = "@demo.medicare.com";
    private const string DemoPatientPassword = "demo123!";

    public static async Task SeedAsync(DatabaseContext context)
    {
        await SeedAdminAsync(context);

        if (await context.Users.AnyAsync(u => u.Email.EndsWith(DemoPatientDomain)))
        {
            Console.WriteLine("ℹ️  Demo data: already present, nothing added (admin account checked).");
            return;
        }

        var random = new Random(2026);
        var today = DateTime.Today;

        await SeedMedicinesAsync(context);
        await SeedTreatmentsAsync(context);
        await SeedInventoriesAsync(context);
        var patients = await SeedPatientsAsync(context, random, today);
        await SeedOrdersAsync(context, random, today, patients);
        await SeedReservationsAsync(context, random, today, patients);

        Console.WriteLine("✅ Demo data added.");
    }

    private static async Task SeedAdminAsync(DatabaseContext context)
    {
        if (await context.Users.AnyAsync(u => u.Email == DemoAdminEmail))
        {
            Console.WriteLine($"ℹ️  Demo data: {DemoAdminEmail} already exists.");
            return;
        }

        var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
        var hasher = new PasswordHasher<Users>();

        context.Users.Add(new Users
        {
            Email = DemoAdminEmail,
            FirstName = "MediCare",
            LastName = "Admin",
            UserName = "medicare.admin",
            DateOfBirth = new DateTime(1990, 1, 1),
            PhoneNumber = "+38761000000",
            Adress = "Maršala Tita 1",
            City = "Sarajevo",
            Role = adminRole,
            PasswordHash = hasher.HashPassword(null!, DemoAdminPassword),
            IsEnabled = true,
        });
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Demo data: admin {DemoAdminEmail} added.");
    }

    /// <summary>A few extra medicines (only the ones that don't exist yet), using images already in wwwroot/images.</summary>
    private static async Task SeedMedicinesAsync(DatabaseContext context)
    {
        var tablets = await context.MedicineCategories.FirstAsync(c => c.Name == "Tablete");
        var syrup = await context.MedicineCategories.FirstAsync(c => c.Name == "Sirup");

        var medicines = new[]
        {
            new Medicine { Name = "Lexilium", Price = 7.90m, Description = "Anksiolitik za smirenje napetosti i anksioznosti.", MedicineCategoryId = tablets.Id, ImagePath = "/images/Lexilium.jfif", Weight = 3, isEnabled = true },
            new Medicine { Name = "Dalsi", Price = 6.20m, Description = "Sirup za snižavanje temperature i ublažavanje bolova kod djece.", MedicineCategoryId = syrup.Id, ImagePath = "/images/6105bab3-a9c4-43cc-91ab-5ca17f89023f_Dalsi.jfif", Weight = 100, isEnabled = true },
            new Medicine { Name = "Skudexa", Price = 12.40m, Description = "Kombinacija za jake akutne bolove (zubobolja, bolovi nakon operacije).", MedicineCategoryId = tablets.Id, ImagePath = "/images/0effc2fe-c868-44b1-bdd6-ad3e5226a9b3_skudexa-75-mg25-mg-x-15_179069_1_1595159335.jpg.webp", Weight = 75, isEnabled = true },
            new Medicine { Name = "Nalgesin S", Price = 6.80m, Description = "Protiv bolova u zglobovima, mišićima, glavobolje i menstrualnih bolova.", MedicineCategoryId = tablets.Id, ImagePath = "/images/Brufen.png", Weight = 275, isEnabled = true },
        };

        var existing = await context.Medicine.Select(m => m.Name).ToListAsync();
        context.Medicine.AddRange(medicines.Where(m => !existing.Contains(m.Name)));
        await context.SaveChangesAsync();
    }

    private static async Task SeedTreatmentsAsync(DatabaseContext context)
    {
        var checkup = await context.TreatmentCategories.FirstAsync(c => c.CategoryName == "Pregled");
        var urgent = await context.TreatmentCategories.FirstAsync(c => c.CategoryName == "Hitno");

        var treatments = new[]
        {
            new Treatments { ServiceName = "Ortopedija", Price = 70.00m, Description = "Pregled kostiju i zglobova – bolovi u leđima, koljenu, ramenu.", ImagePath = "/images/139958e1-2c14-4681-97e7-4bf14e6f1619_ortopedija Sv Jovan.jpg", TreatmentCategoryId = checkup.Id, isEnabled = true },
            new Treatments { ServiceName = "Opšti pregled", Price = 30.00m, Description = "Opšti ljekarski pregled – temperatura, prehlada, umor, kontrola pritiska.", ImagePath = "/images/db53602f-899c-4e07-90c4-83709372c721_R.jpg", TreatmentCategoryId = checkup.Id, isEnabled = true },
            new Treatments { ServiceName = "Hitna konsultacija", Price = 60.00m, Description = "Brza konsultacija za akutne tegobe koje ne mogu čekati redovan termin.", ImagePath = "/images/db53602f-899c-4e07-90c4-83709372c721_R.jpg", TreatmentCategoryId = urgent.Id, isEnabled = true },
        };

        var existing = await context.Treatments.Select(t => t.ServiceName).ToListAsync();
        context.Treatments.AddRange(treatments.Where(t => !existing.Contains(t.ServiceName)));
        await context.SaveChangesAsync();
    }

    /// <summary>Stock for medicines that have none yet – a few deliberately low, so the "low stock" list isn't empty.</summary>
    private static async Task SeedInventoriesAsync(DatabaseContext context)
    {
        var stock = new Dictionary<string, int>
        {
            ["Paracetamol"] = 6,
            ["Lexilium"] = 45,
            ["Dalsi"] = 3,
            ["Skudexa"] = 0,
            ["Nalgesin S"] = 60,
        };

        var withoutStock = await context.Medicine
            .Where(m => !context.Inventories.Any(i => i.MedicineId == m.Id))
            .ToListAsync();

        foreach (var medicine in withoutStock)
        {
            context.Inventories.Add(new Inventories
            {
                MedicineId = medicine.Id,
                QuantityInStock = stock.GetValueOrDefault(medicine.Name, 50)
            });
        }
        await context.SaveChangesAsync();
    }

    private static async Task<List<Users>> SeedPatientsAsync(DatabaseContext context, Random random, DateTime today)
    {
        var userRole = await context.Roles.FirstAsync(r => r.Name == "User");
        var hasher = new PasswordHasher<Users>();

        (string First, string Last, string City)[] people =
        [
            ("Amra", "Hodžić", "Sarajevo"), ("Marko", "Perić", "Mostar"), ("Lejla", "Begić", "Tuzla"),
            ("Ivan", "Jurić", "Mostar"), ("Emina", "Kovačević", "Zenica"), ("Haris", "Mehić", "Sarajevo"),
            ("Ana", "Marić", "Široki Brijeg"), ("Kenan", "Delić", "Bihać"), ("Sara", "Babić", "Banja Luka"),
            ("Tarik", "Omerović", "Mostar"), ("Maja", "Knežević", "Sarajevo"), ("Edin", "Hasić", "Tuzla"),
        ];

        var passwordHash = hasher.HashPassword(null!, DemoPatientPassword);
        var patients = people.Select((p, i) => new Users
        {
            Email = $"{Plain(p.First)}.{Plain(p.Last)}{DemoPatientDomain}",
            FirstName = p.First,
            LastName = p.Last,
            UserName = $"{Plain(p.First)}.{Plain(p.Last)}",
            DateOfBirth = new DateTime(1960 + random.Next(0, 45), random.Next(1, 13), random.Next(1, 29)),
            PhoneNumber = $"+3876{random.Next(1, 7)}{random.Next(100000, 999999)}",
            Adress = $"Ulica {i + 1}",
            City = p.City,
            Role = userRole,
            PasswordHash = passwordHash,
            IsEnabled = true,
        }).ToList();

        context.Users.AddRange(patients);
        await context.SaveChangesAsync();

        // CreatedAtUtc is set to "now" by the audit logic – spread it over the year for the "new users" tile.
        // ExecuteUpdate bypasses the change tracker, so the audit logic doesn't overwrite it again.
        foreach (var patient in patients)
        {
            var createdAt = today.AddDays(-random.Next(0, 365)).ToUniversalTime();
            await context.Users
                .Where(u => u.Id == patient.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.CreatedAtUtc, createdAt));
        }

        return patients;
    }

    private static async Task SeedOrdersAsync(DatabaseContext context, Random random, DateTime today, List<Users> patients)
    {
        var statuses = await context.OrderStatus.ToDictionaryAsync(s => s.StatusName, s => s.Id);
        var medicines = await context.Medicine.Where(m => m.isEnabled).ToListAsync();

        var orders = new List<Orders>();
        for (int i = 0; i < 160; i++)
        {
            // More orders in recent months (business is growing)
            var daysAgo = (int)(365 * Math.Pow(random.NextDouble(), 1.4));
            var orderDate = today.AddDays(-daysAgo);

            var items = medicines
                .OrderBy(_ => random.Next())
                .Take(random.Next(1, 4))
                .Select(m =>
                {
                    var quantity = random.Next(1, 4);
                    return new OrderItems { MedicineId = m.Id, Quantity = quantity, Price = m.Price * quantity };
                })
                .ToList();

            orders.Add(new Orders
            {
                UserId = patients[random.Next(patients.Count)].Id,
                OrderDate = orderDate,
                OrderStatusId = statuses[OrderStatusFor(daysAgo, random)],
                OrderItems = items,
                TotalPrice = items.Sum(x => x.Price),
            });
        }

        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();
    }

    /// <summary>Old orders are mostly finished, recent ones are still in progress.</summary>
    private static string OrderStatusFor(int daysAgo, Random random)
    {
        var roll = random.Next(100);
        if (daysAgo > 14)
            return roll < 75 ? "COMPLETED" : roll < 87 ? "PAID" : "CANCELLED";

        return roll < 35 ? "CONFIRMED" : roll < 65 ? "PAID" : roll < 85 ? "DRAFT" : "CANCELLED";
    }

    /// <summary>
    /// Reservations follow the real booking rules (working days, 08:00–17:00 every 30 min)
    /// and never take a slot that is already booked for the same treatment.
    /// </summary>
    private static async Task SeedReservationsAsync(DatabaseContext context, Random random, DateTime today, List<Users> patients)
    {
        var statuses = await context.OrderStatus.ToDictionaryAsync(s => s.StatusName, s => s.Id);
        var treatments = await context.Treatments.Where(t => t.isEnabled).ToListAsync();

        var taken = (await context.Reservations
                .Select(r => new { r.TreatmentId, r.ReservationDate, r.ReservationTime })
                .ToListAsync())
            .Select(r => (r.TreatmentId, r.ReservationDate.Date, r.ReservationTime))
            .ToHashSet();

        string[] notes =
        [
            "Bol u donjem dijelu leđa već sedmicu dana.",
            "Kontrola nakon terapije.",
            "Glavobolje i vrtoglavica.",
            "Bol u koljenu nakon trčanja.",
            "Povišena temperatura dva dana.",
            "Redovna kontrola šećera.",
        ];

        var reservations = new List<Reservations>();
        int attempts = 0;
        while (reservations.Count < 140 && attempts++ < 2000)
        {
            // Mostly past appointments, some in the next 30 days
            var offset = random.Next(100) < 85 ? -random.Next(1, 366) : random.Next(1, 31);
            var date = today.AddDays(offset);

            // Working days only; Monday and Tuesday are the busiest
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                continue;
            if ((date.DayOfWeek is DayOfWeek.Thursday or DayOfWeek.Friday) && random.Next(100) < 35)
                continue;

            var time = new TimeSpan(8, 0, 0) + TimeSpan.FromMinutes(30 * random.Next(0, 19)); // 08:00–17:00
            var treatment = treatments[random.Next(treatments.Count)];

            if (!taken.Add((treatment.Id, date, time)))
                continue;

            reservations.Add(new Reservations
            {
                UserId = patients[random.Next(patients.Count)].Id,
                TreatmentId = treatment.Id,
                ReservationDate = date,
                ReservationTime = time,
                OrderStatusId = statuses[ReservationStatusFor(offset, random)],
                Price = treatment.Price,
                Notes = random.Next(100) < 40 ? notes[random.Next(notes.Length)] : null,
            });
        }

        context.Reservations.AddRange(reservations);
        await context.SaveChangesAsync();
    }

    private static string ReservationStatusFor(int dayOffset, Random random)
    {
        var roll = random.Next(100);
        if (dayOffset < 0)
            return roll < 85 ? "COMPLETED" : "CANCELLED";

        return roll < 80 ? "CONFIRMED" : "DRAFT";
    }

    /// <summary>"Hodžić" → "hodzic" for e-mail addresses.</summary>
    private static string Plain(string value) =>
        value.ToLowerInvariant()
            .Replace("č", "c").Replace("ć", "c").Replace("š", "s").Replace("ž", "z").Replace("đ", "dj")
            .Replace(" ", "");
}
