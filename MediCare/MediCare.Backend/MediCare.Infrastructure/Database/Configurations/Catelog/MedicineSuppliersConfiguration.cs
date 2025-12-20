using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MediCare.Domain.Entities.HospitalRecords;

public class MedicineSuppliersConfiguration : IEntityTypeConfiguration<MedicineSuppliers>
{
    public void Configure(EntityTypeBuilder<MedicineSuppliers> builder)
    {
        builder.ToTable("MedicineSuppliers");

        builder.HasKey(ms => ms.Id);

        builder.HasOne(ms => ms.Medicine)
            .WithMany()
            .HasForeignKey(ms => ms.MedicineId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(ms => ms.Supplier)
            .WithMany()
            .HasForeignKey(ms => ms.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
