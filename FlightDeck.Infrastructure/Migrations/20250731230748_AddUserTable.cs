using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FlightDeck.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserProgress",
                table: "UserProgress");

            migrationBuilder.DeleteData(
                table: "Airports",
                keyColumn: "IataCode",
                keyValue: "ATL");

            migrationBuilder.DeleteData(
                table: "Airports",
                keyColumn: "IataCode",
                keyValue: "DEN");

            migrationBuilder.DeleteData(
                table: "Airports",
                keyColumn: "IataCode",
                keyValue: "DFW");

            migrationBuilder.DeleteData(
                table: "Airports",
                keyColumn: "IataCode",
                keyValue: "JFK");

            migrationBuilder.DeleteData(
                table: "Airports",
                keyColumn: "IataCode",
                keyValue: "LAS");

            migrationBuilder.DeleteData(
                table: "Airports",
                keyColumn: "IataCode",
                keyValue: "LAX");

            migrationBuilder.DeleteData(
                table: "Airports",
                keyColumn: "IataCode",
                keyValue: "MIA");

            migrationBuilder.DeleteData(
                table: "Airports",
                keyColumn: "IataCode",
                keyValue: "ORD");

            migrationBuilder.DeleteData(
                table: "Airports",
                keyColumn: "IataCode",
                keyValue: "PHX");

            migrationBuilder.DeleteData(
                table: "Airports",
                keyColumn: "IataCode",
                keyValue: "SEA");

            migrationBuilder.AlterColumn<string>(
                name: "AirportCode",
                table: "UserProgress",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserProgress",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserProgress",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "Region",
                table: "Airports",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserProgress",
                table: "UserProgress",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProgress_UserId",
                table: "UserProgress",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProgress_Users_UserId",
                table: "UserProgress",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProgress_Users_UserId",
                table: "UserProgress");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserProgress",
                table: "UserProgress");

            migrationBuilder.DropIndex(
                name: "IX_UserProgress_UserId",
                table: "UserProgress");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserProgress");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserProgress",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "AirportCode",
                table: "UserProgress",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "Region",
                table: "Airports",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserProgress",
                table: "UserProgress",
                columns: new[] { "UserId", "AirportCode" });

            migrationBuilder.InsertData(
                table: "Airports",
                columns: new[] { "IataCode", "City", "Country", "Name", "Region" },
                values: new object[,]
                {
                    { "ATL", "Atlanta", "USA", "Hartsfield-Jackson Atlanta International", "North America" },
                    { "DEN", "Denver", "USA", "Denver International", "North America" },
                    { "DFW", "Dallas", "USA", "Dallas/Fort Worth International", "North America" },
                    { "JFK", "New York", "USA", "John F. Kennedy International", "North America" },
                    { "LAS", "Las Vegas", "USA", "McCarran International", "North America" },
                    { "LAX", "Los Angeles", "USA", "Los Angeles International", "North America" },
                    { "MIA", "Miami", "USA", "Miami International", "North America" },
                    { "ORD", "Chicago", "USA", "O'Hare International", "North America" },
                    { "PHX", "Phoenix", "USA", "Phoenix Sky Harbor International", "North America" },
                    { "SEA", "Seattle", "USA", "Seattle-Tacoma International", "North America" }
                });
        }
    }
}
