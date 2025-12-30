using MediCare.Domain.Entities.HospitalRecords;
using Microsoft.IdentityModel.Protocols;
using System.Runtime.CompilerServices;

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

        await SeedRolesAsync(context);
        await SeedUsersAsync(context);
        await SeedMedicineCategoriesAsync(context);
        await SeedTreatmentCategoriesAsync(context);
        await SeedMedicinesAsync(context);
        await SeedTreatmentsAsync(context);
        await SeedSuppliersAsync(context);
        await SeedMedicineSuppliersAsync(context);
        await SeedInventoriesAsync(context);
        await SeedReceivingsAsync(context);
        await SeedReceivingItemsAsync(context);
        await SeedOrderStatusAsync(context);
        await SeedOrdersAsync(context);
        await SeedPaymentStatusAsync(context);
        await SeedPaymentsAsync(context);
        await SeedCartsAsync(context);
        await SeedCartItemsAsync(context);
        await SeedSavedItemsAsync(context);
        await SeedFavouritesAsync(context);
        await SeedMedicineReviewsAsync(context);
        await SeedReservationAsync(context);
        await SeedReservationReviewsAsync(context);
    }

    private static async Task SeedReservationReviewsAsync(DatabaseContext context)
    {
        if (!await context.ReservationReviews.AnyAsync())
        {
            context.ReservationReviews.AddRange(
               new ReservationReviews
               {
                   UserId = 1,
                   ReservationId = 1,
                   Rating = 5,
                   Comment = "Top",
                   ReviewDate = new DateTime(2024, 3, 7)
               },
               new ReservationReviews
               {
                   UserId = 2,
                   ReservationId = 2,
                   Rating = 5,
                   Comment = "Odlicno",
                   ReviewDate = new DateTime(2024, 3, 7)
               }
            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: Cart items added.");
        }
    }
    private static async Task SeedReservationAsync(DatabaseContext context)
    {
        if (!await context.Reservations.AnyAsync()) {
            context.Reservations.AddRange(
               new Reservations
               {
                   UserId=1,
                   TreatmentId=1,
                   ReservationDate=new DateTime(2025,11,11),
                   Status="Reserved"
               },
               new Reservations
               {
                   UserId = 2,
                   TreatmentId = 1,
                   ReservationDate = new DateTime(2025, 10, 10),
                   Status = "Done"
               }
            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: Cart items added.");
        }
    }
    private static async Task SeedCartItemsAsync(DatabaseContext context)
    {
        if (!await context.CartItems.AnyAsync())
        {
            context.CartItems.AddRange(
                new CartItems
                {
                    CartId=1,
                    MedicineId=1,
                    Quantity=3
                },
                new CartItems
                {
                    CartId = 2,
                    MedicineId = 1,
                    Quantity = 5
                }
            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: Cart items added.");
        }
    }
    private static async Task SeedCartsAsync(DatabaseContext context)
    {
        if (!await context.Carts.AnyAsync())
        {
            context.Carts.AddRange(
                new Carts
                {
                    UserId = 1
                },
                new Carts
                {
                    UserId = 2
                }
            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: Carts added.");
        }
    }
    private static async Task SeedMedicineReviewsAsync(DatabaseContext context)
    {
        if (!await context.MedicineReviews.AnyAsync())
        {
            context.MedicineReviews.AddRange(
                new MedicineReviews
                {
                    UserId = 1,
                    MedicineId = 1,
                    Rating=5,
                    Comment="Top",
                    ReviewDate=new DateTime(2024,3,7)
                },
                new MedicineReviews
                {
                    UserId = 2,
                    MedicineId = 2,
                    Rating = 5,
                    Comment = "Odlicno",
                    ReviewDate = new DateTime(2024, 7, 17)
                }
            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: Medicine reviews added.");
        }
    }
    private static async Task SeedFavouritesAsync(DatabaseContext context)
    {
        if (!await context.Favourites.AnyAsync())
        {
            context.Favourites.AddRange(
                new Favourites
                {
                    UserId = 1,
                    MedicineId = 2

                },
                new Favourites
                {
                    UserId = 2,
                    MedicineId = 1
                }
            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: Favourites added.");
        }
    }
    private static async Task SeedSavedItemsAsync(DatabaseContext context) 
    {
        if (!await context.SavedItems.AnyAsync())
        {
            context.SavedItems.AddRange(
                new SavedItems {
                    UserId=1,
                    MedicineId=1,
                    Quantity=10
                },
                new SavedItems
                {
                    UserId = 2,
                    MedicineId = 1,
                    Quantity = 3
                }
            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: Saved items added.");
        }
    }
    private static async Task SeedReceivingItemsAsync(DatabaseContext context)
    {
        if(!await context.ReceivingItems.AnyAsync())
        {
            context.ReceivingItems.AddRange(
                new ReceivingItems
                {
                    ReceivingId=1,
                    MedicineId=1,
                    Quantity=30,
                    InventoryId=1
                },
                new ReceivingItems
                {
                    ReceivingId = 2,
                    MedicineId = 2,
                    Quantity = 40,
                    InventoryId = 2
                }
                
            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: Receiving items added.");
        }
    }
    private static async Task SeedReceivingsAsync(DatabaseContext context)
    {
        if(!await context.Receivings.AnyAsync())
        {
            context.Receivings.AddRange(
                new Receivings
                {
                    ReceivedDate = new DateTime(2025, 6, 5),
                    SupplierId = 1
                },
                new Receivings
                {
                    ReceivedDate = new DateTime(2025, 11, 5),
                    SupplierId = 2
                }
            );


            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: Receivings added.");
        }
    }
    private static async Task SeedInventoriesAsync(DatabaseContext context) {
        if (!await context.Inventories.AnyAsync()) {
            context.Inventories.AddRange(
                new Inventories
                {
                    MedicineId=1,
                    QuantityInStock=120
                },
                new Inventories
                {
                    MedicineId=2,
                    QuantityInStock=80
                }
            ); 

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: Inventories added.");
        } 
    }
    private static async Task SeedMedicineSuppliersAsync(DatabaseContext context)
    {
        if (!await context.MedicineSuppliers.AnyAsync()) {
            context.MedicineSuppliers.AddRange(
                new MedicineSuppliers
                {
                    SupplierId=1,
                    MedicineId=1
                },
                new MedicineSuppliers
                {
                    SupplierId=2,
                    MedicineId=2
                }
            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: Medicine and Suppliers added.");
        }
    }
    private static async Task SeedSuppliersAsync(DatabaseContext context)
    {
        if (!await context.Suppliers.AnyAsync())
        {
            context.Suppliers.AddRange(
                new Suppliers
                {
                    CompanyName="Pfizer",
                    ContactName="Hans",
                    Phone="061 111-111",
                    Address= "Adresa 1"
                },
                new Suppliers
                {
                    CompanyName = "Bayer",
                    ContactName = "Miki",
                    Phone = "062 222-222",
                    Address = "Adresa 2"
                }
            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: Suppliers added.");
        }
    }
    private static async Task SeedOrderStatusAsync(DatabaseContext context)
    {
        if (!await context.OrderStatus.AnyAsync())
        {
            context.OrderStatus.AddRange(
                new OrderStatus { StatusName = "Paid" },
                new OrderStatus { StatusName = "Processing" },
                new OrderStatus { StatusName = "Pending" },
                new OrderStatus { StatusName = "Failed" }

            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: order status added.");
        }
    }
    public static async Task SeedOrderAndItemsAsync(DatabaseContext context)
    {
        if (await context.Orders.AnyAsync())
            return; // seed je već pokrenut

        // 1️⃣ Kreiramo ordere s itemima
        var orders = new List<Orders>
        {
            new Orders
            {
                UserId = 1, // mora postojati u Users
                OrderDate = new DateTime(2025, 11, 5),
                OrderStatusId = 1, // Pending
                OrderItems = new List<OrderItems>
                {
                    new OrderItems { MedicineId = 1, Price = 10.5m, Quantity = 2 },
                    new OrderItems { MedicineId = 2, Price = 20m, Quantity = 1 }
                }
            },
            new Orders
            {
                UserId = 2,
                OrderDate = new DateTime(2025, 11, 6),
                OrderStatusId = 2, // Completed
                OrderItems = new List<OrderItems>
                {
                    new OrderItems { MedicineId = 2, Price = 15m, Quantity = 3 }
                }
            },
            new Orders
            {
                UserId = 1,
                OrderDate = new DateTime(2025, 11, 7),
                OrderStatusId = 1, // Pending
                OrderItems = new List<OrderItems>
                {
                    new OrderItems { MedicineId = 1, Price = 10.5m, Quantity = 1 },
                    new OrderItems { MedicineId = 2, Price = 30m, Quantity = 2 }
                }
            },
        };

        // 2️⃣ Automatski računamo TotalPrice za svaki order
        foreach (var order in orders)
        {
            order.TotalPrice = order.OrderItems.Sum(i => i.Price * i.Quantity);
        }

        // 3️⃣ Dodajemo u bazu
        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Dynamic seed: orders and order items added.");
    }
    private static async Task SeedPaymentStatusAsync(DatabaseContext context)
    {
        if(!await context.PaymentStatus.AnyAsync())
        {
            context.PaymentStatus.AddRange(
                new PaymentStatus { StatusName = "Paid" },
                new PaymentStatus { StatusName = "Processing" },
                new PaymentStatus { StatusName = "Pending" },
                new PaymentStatus { StatusName = "Failed" }

            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: payment status added.");
        }
    }
    private static async Task SeedPaymentsAsync(DatabaseContext context)
    {
        if(!await context.Payments.AnyAsync())
        {
            context.Payments.AddRange(
                new Payments { 
                    OrderId=2,
                    PaymentDate=new DateTime(2025,11,1),
                    Amount=100,
                    PaymentMethod="Card",
                    TransactionId="123412512465712",
                    PaymentStatusId=1
                }    
            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: payments added.");
        }
    }
    private static async Task SeedMedicineCategoriesAsync(DatabaseContext context)
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
    private static async Task SeedTreatmentCategoriesAsync(DatabaseContext context)
    {
        if (!await context.TreatmentCategories.AnyAsync())
        {
            context.TreatmentCategories.AddRange(
                new TreatmentCategories
                {
                    CategoryName = "Pregled",
                    isEnabled = true,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new TreatmentCategories
                {
                    CategoryName = "Hitno",
                    isEnabled = true,
                    CreatedAtUtc = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: treatment categories added.");
        }
    }
    private static async Task SeedRolesAsync(DatabaseContext context)
    {
        {
            if (!await context.Roles.AnyAsync())
            {
                context.Roles.AddRange(
                    new Roles
                    {
                        Name = "Admin",
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new Roles
                    {
                        Name = "User",
                        CreatedAtUtc = DateTime.UtcNow
                    }
                );

                await context.SaveChangesAsync();
                Console.WriteLine("✅ Dynamic seed: roles added.");
            }
        }
    }
    private static async Task SeedUsersAsync(DatabaseContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        var hasher = new PasswordHasher<Users>();

        var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
        var userRole = await context.Roles.FirstAsync(r => r.Name == "User");

        var pass1 = "Admin";

        var admin = new Users
        {
            Email = "admin@market.com",
            FirstName = "Renato",
            LastName = "Antunovic",
            UserName = "admin1",
            PhoneNumber = "061-111-111",
            Password = pass1,
            Adress = "Adresa1",
            City = "Grad1",
            RoleId = 1,
            Role = adminRole,
            PasswordHash = hasher.HashPassword(null!, pass1),
            IsEnabled = true,
        };

        var user = new Users
        {
            Email = "manager@market.local",
            FirstName = "user1",
            LastName = "user1",
            UserName = "admin1",
            PhoneNumber = "062-222-222",
            Password = "user1",
            Adress = "Adresa2",
            City = "Grad2",
            RoleId = 1,
            Role = adminRole,
            PasswordHash = hasher.HashPassword(null!, "User123!"),
            IsEnabled = true,
        };

        var dummyForSwagger = new Users
        {
            Email = "string",
            FirstName = "user2",
            LastName = "user2",
            UserName = "admin2",
            PhoneNumber = "063-333-333",
            Password = "Admin3",
            Adress = "Adresa3",
            City = "Grad3",
            RoleId = 1,
            Role = adminRole,
            PasswordHash = hasher.HashPassword(null!, "string"),
            IsEnabled = true,
        };
        var dummyForTests = new Users
        {
            Email = "test",
            FirstName = "user4",
            LastName = "user4",
            UserName = "user2",
            PhoneNumber = "063-333-333",
            Password = "Admin3",
            Adress = "Adresa3",
            City = "Grad3",
            RoleId = 2,
            Role = userRole,
            PasswordHash = hasher.HashPassword(null!, "test123"),
            IsEnabled = true,
        };
        context.Users.AddRange(admin, user, dummyForSwagger, dummyForTests);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Dynamic seed: demo users added.");
    }
    private static async Task SeedTreatmentsAsync(DatabaseContext context)
    {
        if (!await context.Treatments.AnyAsync())
        {
            context.Treatments.AddRange(
                new Treatments
                {
                    ServiceName = "Endokrinologija",
                    Price = 50.00m,
                    Description = "Dijabetes i zljezde.",
                    ImagePath = "/images/Endokrinologija.jpg",
                    TreatmentCategoryId = 1, // pretpostavljamo da postoji kategorija sa Id = 1
                    isEnabled = true
                },
                new Treatments
                {
                    ServiceName = "Ginekologija",
                    Price = 80.00m,
                    Description = "Zenski reproduktivni organi",
                    ImagePath = "/images/Ginekologija.jfif",
                    TreatmentCategoryId = 2, // pretpostavljamo da postoji kategorija sa Id = 2
                    isEnabled = true
                }
            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: Treatments added.");
        }
    }
    private static async Task SeedMedicinesAsync(DatabaseContext context)
    {
        if (!await context.Medicine.AnyAsync())
        {
            context.Medicine.AddRange(
                new Medicine
                {
                    Name = "Brufen",
                    Price = 5.50m,
                    Description = "Pain reliever and anti-inflammatory medication.",
                    MedicineCategoryId = 1, // pretpostavljamo da postoji kategorija sa Id = 1
                    ImagePath = "/images/Brufen.png",
                    Weight = 200, // mg
                    isEnabled = true
                },
                new Medicine
                {
                    Name = "Aspirin",
                    Price = 3.00m,
                    Description = "Used to reduce pain, fever, or inflammation.",
                    MedicineCategoryId = 1,
                    ImagePath = "/images/Aspirin.webp",
                    Weight = 100, // mg
                    isEnabled = true
                },
                new Medicine
                {
                    Name = "Paracetamol",
                    Price = 2.50m,
                    Description = "Commonly used for pain relief and fever reduction.",
                    MedicineCategoryId = 1,
                    ImagePath = "/images/Paracetamol.jfif",
                    Weight = 500, // mg
                    isEnabled = true
                }
            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: Medicines added.");
        }
    }
    private static async Task SeedOrdersAsync(DatabaseContext context)
    {
        if (await context.Orders.AnyAsync())
            return;

        // Dohvati korisnike koji postoje
        var userAdmin = await context.Users.FirstAsync(u => u.Email == "admin@market.com");
        var userManager = await context.Users.FirstAsync(u => u.Email == "manager@market.local");

        // Dohvati statuse narudžbi
        var statusPending = await context.OrderStatus.FirstAsync(s => s.StatusName == "Pending");
        var statusProcessing = await context.OrderStatus.FirstAsync(s => s.StatusName == "Processing");

        // Dohvati lijekove
        var medBrufen = await context.Medicine.FirstAsync(m => m.Name == "Brufen");
        var medAspirin = await context.Medicine.FirstAsync(m => m.Name == "Aspirin");
        var medParacetamol = await context.Medicine.FirstAsync(m => m.Name == "Paracetamol");

        // Kreiraj narudžbe
        var orders = new List<Orders>
    {
        new Orders
        {
            UserId = userAdmin.Id,
            OrderStatusId = statusPending.Id,
            OrderDate = new DateTime(2025, 11, 5),
            OrderItems = new List<OrderItems>
            {
                new OrderItems { MedicineId = medBrufen.Id, Quantity = 2, Price = 10.5m },
                new OrderItems { MedicineId = medAspirin.Id, Quantity = 1, Price = 20m }
            }
        },
        new Orders
        {
            UserId = userManager.Id,
            OrderStatusId = statusProcessing.Id,
            OrderDate = new DateTime(2025, 11, 6),
            OrderItems = new List<OrderItems>
            {
                new OrderItems { MedicineId = medParacetamol.Id, Quantity = 3, Price = 15m }
            }
        }
    };

        // Izračunaj ukupnu cijenu za svaku narudžbu
        foreach (var order in orders)
        {
            order.TotalPrice = order.OrderItems.Sum(i => i.Quantity * i.Price);
        }

        // Dodaj u kontekst i snimi
        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Dynamic seed: Orders added.");
    }

}