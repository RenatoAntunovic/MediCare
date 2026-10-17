using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReservationSlotUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_TreatmentId",
                table: "Reservations");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Slot",
                table: "Reservations",
                columns: new[] { "TreatmentId", "ReservationDate", "ReservationTime" },
                unique: true,
                filter: "[OrderStatusId] <> 5 AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_Slot",
                table: "Reservations");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_TreatmentId",
                table: "Reservations",
                column: "TreatmentId");
        }
    }
}
