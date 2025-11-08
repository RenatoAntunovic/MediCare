namespace MediCare.Application.Abstractions;

// Application layer
public interface IAppDbContext
{
    DbSet<Users> Users { get; }
    DbSet<RefreshTokenEntity> RefreshTokens { get; }
    DbSet<Inventories> Inventories { get; }
    DbSet<Medicine> Medicine { get; }
    DbSet<MedicineSuppliers> MedicineSuppliers { get; }
    DbSet<Receivings> Receivings { get; }
    DbSet<ReceivingItems> ReceivingItems { get; }
    DbSet<Suppliers> Suppliers { get; }
    DbSet<CartItems> CartItems { get; }
    DbSet<Carts> Carts { get; }
    DbSet<Favourites> Favourites { get; }
    DbSet<MedicineCategories> MedicineCategories { get; }
    DbSet<MedicineReviews> MedicineReviews { get; }
    DbSet<OrderItems> OrderItems { get; }
    DbSet<Orders> Orders { get; }
    DbSet<OrderStatus> OrderStatus { get; }
    DbSet<Payments> Payments { get; }
    DbSet<PaymentStatus> PaymentStatus { get; }
    DbSet<ProductReviews> ProductReviews { get; }
    DbSet<ReservationReviews> ReservationReviews { get; }
    DbSet<Reservations> Reservations { get; }
    DbSet<Roles> Roles { get; }
    DbSet<SavedItems> SavedItems { get; }
    DbSet<TreatmentCategories> TreatmentCategories { get; }
    DbSet<Treatments> Treatments { get; }

    Task<int> SaveChangesAsync(CancellationToken ct);
}