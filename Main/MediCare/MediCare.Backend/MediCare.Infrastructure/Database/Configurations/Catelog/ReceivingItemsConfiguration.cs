using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MediCare.Domain.Entities.Catalog;

public class ReceivingItemsConfiguration : IEntityTypeConfiguration<ReceivingItems>
{
    public void Configure(EntityTypeBuilder<ReceivingItems> builder)
    {
        builder.ToTable("ReceivingItems");

        builder.HasKey(ri => ri.Id);

        builder.Property(ri => ri.Quantity)
            .IsRequired();

        builder.HasOne(ri => ri.Receiving)
            .WithMany(r => r.Items)
            .HasForeignKey(ri => ri.ReceivingId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(ri => ri.Medicine)
            .WithMany(m => m.ReceivingItems)
            .HasForeignKey(ri => ri.MedicineId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(ri => ri.Inventory)
            .WithMany(i => i.ReceivingItems)
            .HasForeignKey(ri => ri.InventoryId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
