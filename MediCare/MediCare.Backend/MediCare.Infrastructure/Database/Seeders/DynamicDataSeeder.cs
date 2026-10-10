using MediCare.Domain.Entities.HospitalRecords;
using Microsoft.IdentityModel.Protocols;
using System.Runtime.CompilerServices;

namespace MediCare.Infrastructure.Database.Seeders;

/// <summary>
/// Dynamic seeder that runs at runtime,
/// usually when the application starts (e.g. in Program.cs).
/// Used to insert demo/test data that is not part of the migrations.
/// </summary>
public static class DynamicDataSeeder
{
    public static async Task SeedAsync(DatabaseContext context)
    {
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
        //await SeedReservationReviewsAsync(context);
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
        if (!await context.Reservations.AnyAsync())
        {
            context.Reservations.AddRange(
               new Reservations
               {
                   UserId = 1,
                   TreatmentId = 1,
                   ReservationDate = new DateTime(2025, 11, 11),
                   ReservationTime = new TimeSpan(14, 0, 0), // 14:00
                   OrderStatusId = 2, // CONFIRMED
                   Price = 50.00m
               },
               new Reservations
               {
                   UserId = 2,
                   TreatmentId = 1,
                   ReservationDate = new DateTime(2025, 10, 10),
                   ReservationTime = new TimeSpan(10, 30, 0), // 10:30
                   OrderStatusId = 4, // COMPLETED (Done)
                   Price = 50.00m
               }
            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: Reservations added.");
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
                new OrderStatus { StatusName = "DRAFT" },      // Id 1
                new OrderStatus { StatusName = "CONFIRMED" },  // Id 2
                new OrderStatus { StatusName = "PAID" },       // Id 3
                new OrderStatus { StatusName = "COMPLETED" },  // Id 4
                new OrderStatus { StatusName = "CANCELLED" }   // Id 5
            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: order status added.");
        }
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
    /// <summary>
    /// Two demo accounts: one admin (Id 1) and one regular user (Id 2).
    /// The other seeds (cart, favourites, orders, reservations) use UserId 1 and 2.
    /// </summary>
    private static async Task SeedUsersAsync(DatabaseContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        var hasher = new PasswordHasher<Users>();

        var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
        var userRole = await context.Roles.FirstAsync(r => r.Name == "User");

        var admin = new Users
        {
            Email = "admin@market.com",
            FirstName = "admin",
            LastName = "lastname",
            UserName = "admin",
            PhoneNumber = "061-111-111",
            Adress = "Adresa 1",
            City = "Mostar",
            Role = adminRole,
            PasswordHash = hasher.HashPassword(null!, "Admin"),
            IsEnabled = true,
        };

        var user = new Users
        {
            Email = "client@gmail.com",
            FirstName = "client",
            LastName = "lastname",
            UserName = "user",
            PhoneNumber = "062-222-222",
            Adress = "Adresa 2",
            City = "Mostar",
            Role = userRole,
            PasswordHash = hasher.HashPassword(null!, "test123"),
            IsEnabled = true,
        };

        // Added one by one so the Ids are guaranteed: admin = 1, user = 2
        context.Users.Add(admin);
        await context.SaveChangesAsync();
        context.Users.Add(user);
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
                    TreatmentCategoryId = 1, // assumes a category with Id = 1 exists
                    isEnabled = true
                },
                new Treatments
                {
                    ServiceName = "Ginekologija",
                    Price = 80.00m,
                    Description = "Zenski reproduktivni organi",
                    ImagePath = "/images/Ginekologija.jfif",
                    TreatmentCategoryId = 2, // assumes a category with Id = 2 exists
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
                    MedicineCategoryId = 1, // assumes a category with Id = 1 exists
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

        // Get existing users
        var userAdmin = await context.Users.FirstAsync(u => u.Email == "admin@market.com");
        var userManager = await context.Users.FirstAsync(u => u.Email == "client@gmail.com");

        // Get order statuses
        var statusPending = await context.OrderStatus.FirstAsync(s => s.StatusName == "DRAFT");
        var statusProcessing = await context.OrderStatus.FirstAsync(s => s.StatusName == "CONFIRMED");
        // Get medicines
        var medBrufen = await context.Medicine.FirstAsync(m => m.Name == "Brufen");
        var medAspirin = await context.Medicine.FirstAsync(m => m.Name == "Aspirin");
        var medParacetamol = await context.Medicine.FirstAsync(m => m.Name == "Paracetamol");

        // Create orders
        var orders = new List<Orders>
    {
        new Orders
        {
            UserId = userAdmin.Id,
            OrderStatusId = statusPending.Id,
            OrderDate = new DateTime(2025, 11, 5),
            OrderItems = new List<OrderItems>
            {
                new OrderItems { MedicineId = medBrufen.Id, Quantity = 2, Price = medBrufen.Price * 2 },
                new OrderItems { MedicineId = medAspirin.Id, Quantity = 1, Price = medAspirin.Price * 1 }
            }
        },
        new Orders
        {
            UserId = userManager.Id,
            OrderStatusId = statusProcessing.Id,
            OrderDate = new DateTime(2025, 11, 6),
            OrderItems = new List<OrderItems>
            {
                new OrderItems { MedicineId = medParacetamol.Id, Quantity = 3, Price = medParacetamol.Price * 3 }
            }
        }
    };

        // Calculate the total price for each order
        foreach (var order in orders)
        {
            order.TotalPrice = order.OrderItems.Sum(i => i.Price);
        }

        // Add to the context and save
        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Dynamic seed: Orders added.");
    }

}