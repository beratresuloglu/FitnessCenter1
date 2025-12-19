using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessCenterWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class FixTrainerAvailabilityColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsAvailable",
                table: "TrainerAvailabilities",
                newName: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "TrainerAvailabilities",
                newName: "IsAvailable");
        }
    }
}
