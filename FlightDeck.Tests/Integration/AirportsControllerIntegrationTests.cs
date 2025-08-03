using FlightDeck.Core.Models;
using FlightDeck.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace FlightDeck.Tests.Integration;

/// <summary>
/// Integration tests for AirportsController - tests the full HTTP pipeline
/// These tests actually make HTTP requests to your API and test end-to-end behavior
/// </summary>
public class AirportsControllerIntegrationTests : IClassFixture<FlightDeckWebApplicationFactory>
{
    private readonly FlightDeckWebApplicationFactory _factory;

    public AirportsControllerIntegrationTests(FlightDeckWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAirports_WithSeededData_ReturnsAllAirports()
    {
        // Arrange - Set up test data
        await _factory.SeedTestDataAsync();
        var client = _factory.CreateClient();

        // Act - Make actual HTTP GET request
        var response = await client.GetAsync("/api/airports");

        // Assert - Verify the HTTP response
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Deserialize the JSON response
        var airports = await response.Content.ReadFromJsonAsync<List<Airport>>();
        
        airports.Should().NotBeNull();
        airports.Should().HaveCount(4); // Updated to match our 4 test airports
        airports.Should().Contain(a => a.IataCode == "LAX");
        airports.Should().Contain(a => a.IataCode == "JFK");
        airports.Should().Contain(a => a.IataCode == "LHR");
        airports.Should().Contain(a => a.IataCode == "ATL");
    }

    [Fact]
    public async Task GetAirports_WithEmptyDatabase_ReturnsEmptyList()
    {
        // Arrange - Start with clean database (reset to ensure empty)
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/airports");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var airports = await response.Content.ReadFromJsonAsync<List<Airport>>();
        airports.Should().NotBeNull();
        airports.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAirport_WithValidData_ReturnsCreatedAirport()
    {
        // Arrange - Start with clean database
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();
        var newAirport = new Airport("DEN", "Denver International Airport", "Denver", "USA", "North America");

        // Act - Make HTTP POST request with JSON body
        var response = await client.PostAsJsonAsync("/api/airports", newAirport);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdAirport = await response.Content.ReadFromJsonAsync<Airport>();
        createdAirport.Should().NotBeNull();
        createdAirport!.IataCode.Should().Be("DEN");
        createdAirport.Name.Should().Be("Denver International Airport");
        
        // Verify it was actually saved to database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FlightDeckDbContext>();
        var savedAirport = await context.Airports.FindAsync("DEN");
        savedAirport.Should().NotBeNull();
        savedAirport!.Name.Should().Be("Denver International Airport");
    }

    [Fact]
    public async Task CreateAirport_WithDuplicateIataCode_ReturnsConflict()
    {
        // Arrange - Seed data first
        await _factory.SeedTestDataAsync();
        var client = _factory.CreateClient();

        var duplicateAirport = new Airport("LAX", "Different Airport", "Different City", "Different Country", "Different Region");

        // Act
        var response = await client.PostAsJsonAsync("/api/airports", duplicateAirport);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        
        var errorMessage = await response.Content.ReadAsStringAsync();
        errorMessage.Should().Contain("already exists");
        errorMessage.Should().Contain("LAX");
    }

    [Fact]
    public async Task CreateAirport_WithInvalidJson_ReturnsBadRequest()
    {
        // Arrange - Send malformed JSON
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();
        var invalidJson = "{ invalid json }";
        var content = new StringContent(invalidJson, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/airports", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAirport_WithMissingRequiredFields_HandlesMissingData()
    {
        // Arrange - Airport with missing required fields
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();
        var incompleteAirport = new
        {
            // Missing IataCode, Name, etc.
            City = "Some City"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/airports", incompleteAirport);

        // Assert - This tests how your API handles incomplete data
        // The exact behavior depends on your model validation
        // For now, let's just verify we get a response
        response.Should().NotBeNull();
        
        // Note: In a real app, you'd add model validation to return 400 Bad Request
        // We'll add this in the validation chapter!
    }

    [Fact]
    public async Task Integration_CreateThenRetrieve_WorksEndToEnd()
    {
        // Arrange - Start with clean database
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();
        var newAirport = new Airport("SEA", "Seattle-Tacoma International Airport", "Seattle", "USA", "North America");

        // Act 1 - Create airport
        var createResponse = await client.PostAsJsonAsync("/api/airports", newAirport);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act 2 - Retrieve all airports
        var getResponse = await client.GetAsync("/api/airports");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert - The created airport should be in the list
        var airports = await getResponse.Content.ReadFromJsonAsync<List<Airport>>();
        airports.Should().Contain(a => 
            a.IataCode == "SEA" && 
            a.Name == "Seattle-Tacoma International Airport");
    }
}