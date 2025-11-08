using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsEnabledColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isEnabled",
                table: "Treatments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isEnabled",
                table: "TreatmentCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isEnabled",
                table: "Medicine",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isEnabled",
                table: "Treatments");

            migrationBuilder.DropColumn(
                name: "isEnabled",
                table: "TreatmentCategories");

            migrationBuilder.DropColumn(
                name: "isEnabled",
                table: "Medicine");
        }
    }
}
