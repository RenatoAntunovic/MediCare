using MediCare.Domain.Entities.Catalog;

namespace MediCare.Infrastructure.Database.Seeders;

/// <summary>
/// Dynamic seeder koji se pokreće u runtime-u,
/// obično pri startu aplikacije (npr. u Program.cs).
/// Koristi se za unos demo/test podataka koji nisu dio migracije.
/// </summary>
public static class DynamicDataSeeder
{
    public static async Task SeedAsync(DatabaseContext context)
    {
        // Osiguraj da baza postoji (bez migracija)
        await context.Database.EnsureCreatedAsync();

        await SeedProductCategoriesAsync(context);
        await SeedUsersAsync(context);
    }

    private static async Task SeedProductCategoriesAsync(DatabaseContext context)
    {
        if (!await context.MedicineCategories.AnyAsync())
        {
            context.MedicineCategories.AddRange(
                new MedicineCategories
                {
                    Name = "Tablete",
                    IsEnabled = true,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new MedicineCategories
                {
                    Name = "Sirup",
                    IsEnabled = true,
                    CreatedAtUtc = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: product categories added.");
        }
    }

    /// <summary>
    /// Kreira demo korisnike ako ih još nema u bazi.
    /// </summary>
    private static async Task SeedUsersAsync(DatabaseContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        var hasher = new PasswordHasher<Users>();

        var admin = new Users
        {
            Email = "admin@market.local",
            PasswordHash = hasher.HashPassword(null!, "Admin123!"),
            IsEnabled = true,
        };

        var user = new Users
        {
            Email = "manager@market.local",
            PasswordHash = hasher.HashPassword(null!, "User123!"),
            IsEnabled = true,
        };

        var dummyForSwagger = new Users
        {
            Email = "string",
            PasswordHash = hasher.HashPassword(null!, "string"),
            IsEnabled = true,
        };
        var dummyForTests = new Users
        {
            Email = "test",
            PasswordHash = hasher.HashPassword(null!, "test123"),
            IsEnabled = true,
        };
        context.Users.AddRange(admin, user, dummyForSwagger, dummyForTests);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Dynamic seed: demo users added.");
    }
}