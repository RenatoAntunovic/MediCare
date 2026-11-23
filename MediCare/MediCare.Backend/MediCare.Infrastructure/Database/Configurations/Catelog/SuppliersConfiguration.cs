using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MediCare.Domain.Entities.HospitalRecords;

public class SuppliersConfiguration : IEntityTypeConfiguration<Suppliers>
{
    public void Configure(EntityTypeBuilder<Suppliers> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.CompanyName)
            .IsRequired()
            .HasMaxLength(Suppliers.Constraints.CompanyNameMaxLength);

        builder.Property(s => s.ContactName)
            .IsRequired()
            .HasMaxLength(Suppliers.Constraints.ContactNameMaxLength);

        builder.Property(s => s.Email)
            .IsRequired()
            .HasMaxLength(Suppliers.Constraints.EmailMaxLength);

        builder.Property(s => s.Phone)
            .IsRequired()
            .HasMaxLength(Suppliers.Constraints.PhoneMaxLength);

        builder.Property(s => s.Address)
            .IsRequired()
            .HasMaxLength(Suppliers.Constraints.AddressMaxLength);

        builder.Property(s => s.City)
            .IsRequired()
            .HasMaxLength(Suppliers.Constraints.CityMaxLength);

        // Existing rows become active after the migration
        builder.Property(s => s.IsEnabled)
            .HasDefaultValue(true);
    }
}
