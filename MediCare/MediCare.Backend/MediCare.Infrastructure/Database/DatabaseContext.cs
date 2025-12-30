using MediCare.Application.Abstractions;

namespace MediCare.Infrastructure.Database;

public partial class DatabaseContext : DbContext, IAppDbContext
{
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
    public DbSet<CartItems> CartItems => Set<CartItems>();
    public DbSet<Carts> Carts => Set<Carts>();
    public DbSet<Favourites> Favourites => Set<Favourites>();
    public DbSet<Inventories> Inventories => Set<Inventories>();
    public DbSet<Medicine> Medicine => Set<Medicine>();
    public DbSet<MedicineCategories> MedicineCategories => Set<MedicineCategories>();
    public DbSet<MedicineReviews> MedicineReviews => Set<MedicineReviews>();
    public DbSet<MedicineSuppliers> MedicineSuppliers => Set<MedicineSuppliers>();
    public DbSet<OrderItems> OrderItems => Set<OrderItems>();
    public DbSet<Orders> Orders => Set<Orders>();
    public DbSet<OrderStatus> OrderStatus => Set<OrderStatus>();
    public DbSet<Payments> Payments => Set<Payments>();
    public DbSet<PaymentStatus> PaymentStatus => Set<PaymentStatus>();
    public DbSet<ForLater> ForLater => Set<ForLater>();
    public DbSet<ReceivingItems> ReceivingItems => Set<ReceivingItems>();
    public DbSet<Receivings> Receivings => Set<Receivings>();
    public DbSet<ReservationReviews> ReservationReviews => Set<ReservationReviews>();
    public DbSet<Reservations> Reservations => Set<Reservations>();
    public DbSet<Roles> Roles => Set<Roles>();
    public DbSet<SavedItems> SavedItems => Set<SavedItems>();
    public DbSet<Suppliers> Suppliers => Set<Suppliers>();
    public DbSet<TreatmentCategories> TreatmentCategories => Set<TreatmentCategories>();
    public DbSet<Treatments> Treatments => Set<Treatments>();
    public DbSet<Users> Users => Set<Users>();

    private readonly TimeProvider _clock;
    public DatabaseContext(DbContextOptions<DatabaseContext> options, TimeProvider clock)
        : base(options)
    {
        _clock = clock;
    }

}