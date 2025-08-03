using FlightDeck.API.Controllers;
using FlightDeck.Core.Models;
using FlightDeck.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace FlightDeck.Tests;

public class AirportsControllerTests
{
    private FlightDeckDbContext CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<FlightDeckDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // New DB for each call
            .Options;

        var context = new FlightDeckDbContext(options);

        // Add fresh test data
        var airports = new List<Airport>
        {
            new("ATL", "Atlanta International", "Atlanta", "USA", "North America"),
            new("LAX", "Los Angeles International", "Los Angeles", "USA", "North America")
        };

        context.Airports.AddRange(airports);
        context.SaveChanges();
        context.ChangeTracker.Clear(); // Important: Clear tracking

        return context;
    }

    [Fact]
    public async Task GetAirports_ReturnsAllAirports()
    {
        // Arrange
        using var context = CreateTestContext();
        var controller = new AirportsController(context);

        // Act
        var result = await controller.GetAirports();

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var airports = actionResult.Value.Should().BeOfType<List<Airport>>().Subject;
        airports.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAirport_WithValidAirport_ReturnsCreatedResult()
    {
        // Arrange
        using var context = CreateTestContext();
        var controller = new AirportsController(context);
        var newAirport = new Airport("JFK", "John F. Kennedy International", "New York", "USA", "North America");

        // Act
        var result = await controller.CreateAirport(newAirport);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateAirport_WithDuplicateCode_ReturnsConflict()
    {
        // Arrange
        using var context = CreateTestContext();
        var controller = new AirportsController(context);
        var duplicateAirport = new Airport("ATL", "Some Other Atlanta Airport", "Atlanta", "USA", "North America");

        // Act
        var result = await controller.CreateAirport(duplicateAirport);

        // Assert
        result.Result.Should().BeOfType<ConflictObjectResult>();
    }
}