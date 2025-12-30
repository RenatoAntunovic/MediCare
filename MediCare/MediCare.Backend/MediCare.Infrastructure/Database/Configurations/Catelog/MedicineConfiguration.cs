namespace MediCare.Infrastructure.Database.Configurations.Catelog;

public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> builder)
    {
        builder
            .ToTable("Medicine");

        builder
            .Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(Medicine.Constraints.NameMaxLength);

        builder
            .Property(x => x.Description)
            .HasMaxLength(Medicine.Constraints.DescriptionMaxLength);

        builder
            .Property(x => x.Price)
            .HasPrecision(18, 2);

        builder
            .Property(x => x.Weight)
            .IsRequired();

        builder
            .HasOne(x => x.MedicineCategory)
            .WithMany(x => x.Medicine)
            .HasForeignKey(x => x.MedicineCategoryId)
            .OnDelete(DeleteBehavior.NoAction);// Restrict — do not allow deleting a category if it has products

        builder.HasMany(m => m.ReceivingItems)
      .WithOne(ri => ri.Medicine)
      .HasForeignKey(ri => ri.MedicineId)
      .OnDelete(DeleteBehavior.NoAction);
    }
}