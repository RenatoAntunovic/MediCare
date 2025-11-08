using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MediCare.Domain.Entities.Catalog;

public class InventoriesConfiguration : IEntityTypeConfiguration<Inventories>
{
    public void Configure(EntityTypeBuilder<Inventories> builder)
    {
        builder.ToTable("Inventories");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.QuantityInStock)
            .IsRequired();

        builder.HasOne(i => i.Medicine)
            .WithOne()
            .HasForeignKey<Inventories>(i => i.MedicineId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(i => i.ReceivingItems)
            .WithOne(ri => ri.Inventory)
            .HasForeignKey(ri => ri.InventoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
