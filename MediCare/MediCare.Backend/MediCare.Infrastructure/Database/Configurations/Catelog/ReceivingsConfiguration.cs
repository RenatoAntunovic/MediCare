using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MediCare.Domain.Entities.HospitalRecords;

public class ReceivingsConfiguration : IEntityTypeConfiguration<Receivings>
{
    public void Configure(EntityTypeBuilder<Receivings> builder)
    {
        builder.ToTable("Receivings");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReceivedDate)
            .IsRequired();

        builder.HasOne(r => r.Supplier)
            .WithMany()
            .HasForeignKey(r => r.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Items)
            .WithOne(ri => ri.Receiving)
            .HasForeignKey(ri => ri.ReceivingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
