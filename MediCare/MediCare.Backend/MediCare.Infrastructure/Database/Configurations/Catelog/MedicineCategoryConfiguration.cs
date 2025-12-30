namespace MediCare.Infrastructure.Database.Configurations.Catelog;

public class MedicineCategoryConfiguration : IEntityTypeConfiguration<MedicineCategories>
{
    public void Configure(EntityTypeBuilder<MedicineCategories> builder)
    {
        builder
            .ToTable("MedicineCategories");

        builder
            .Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(MedicineCategories.Constraints.NameMaxLength);

        builder
            .Property(x => x.IsEnabled)
            .IsRequired();

    }
}
