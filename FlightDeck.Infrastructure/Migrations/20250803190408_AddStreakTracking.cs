using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightDeck.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStreakTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BestStreak",
                table: "UserProgress",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentStreak",
                table: "UserProgress",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BestStreak",
                table: "UserProgress");

            migrationBuilder.DropColumn(
                name: "CurrentStreak",
                table: "UserProgress");
        }
    }
}
