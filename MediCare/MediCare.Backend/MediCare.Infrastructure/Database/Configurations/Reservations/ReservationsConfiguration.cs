using MediCare.Domain.Entities.HospitalRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ReservationsConfiguration : IEntityTypeConfiguration<Reservations>
{
    public const string SlotIndexName = "IX_Reservations_Slot";

    public void Configure(EntityTypeBuilder<Reservations> builder)
    {
        // One ACTIVE reservation per treatment + date + time.
        // Cancelled (OrderStatusId 5 = CANCELLED in the seed) and soft-deleted rows don't block the slot.
        builder.HasIndex(r => new { r.TreatmentId, r.ReservationDate, r.ReservationTime })
            .HasDatabaseName(SlotIndexName)
            .IsUnique()
            .HasFilter("[OrderStatusId] <> 5 AND [IsDeleted] = 0");
    }
}