using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: the column may already exist if an earlier migration (init69) was applied to this database.
            migrationBuilder.Sql(@"
        IF COL_LENGTH('Reservations', 'Notes') IS NULL
            ALTER TABLE [Reservations] ADD [Notes] nvarchar(1000) NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        IF COL_LENGTH('Reservations', 'Notes') IS NOT NULL
            ALTER TABLE [Reservations] DROP COLUMN [Notes];");
        }
    }
}
