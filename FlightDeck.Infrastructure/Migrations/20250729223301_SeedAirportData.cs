using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FlightDeck.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAirportData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
