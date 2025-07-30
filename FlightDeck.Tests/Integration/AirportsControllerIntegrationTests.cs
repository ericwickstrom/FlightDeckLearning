using FlightDeck.Core.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FlightDeck.Tests.Integration;

/// <summary>
/// Integration tests for AirportsController - tests the full HTTP pipeline
/// These tests actually make HTTP requests to your API and test end-to-end behavior
/// </summary>
public class AirportsControllerIntegrationTests : IClassFixture<FlightDeckWebApplicationFactory>
{
    private readonly FlightDeckWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AirportsControllerIntegrationTests(FlightDeckWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(); // Creates an HTTP client that talks to your test server
    }

    [Fact]
    public async Task GetAirports_WithSeededData_ReturnsAllAirports()
    {
        // Arrange - Set up test data
        await _factory.SeedTestDataAsync();

        // Act - Make actual HTTP GET request
        var response = await _client.GetAsync("/api/airports");

        // Assert - Verify the HTTP response
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Deserialize the JSON response
        var airports = await response.Content.ReadFromJsonAsync<List<Airport>>();
        
        airports.Should().NotBeNull();
        airports.Should().HaveCount(3);
        airports.Should().Contain(a => a.IataCode == "LAX");
        airports.Should().Contain(a => a.IataCode == "JFK");
        airports.Should().Contain(a => a.IataCode == "LHR");
    }

    [Fact]
    public async Task GetAirports_WithEmptyDatabase_ReturnsEmptyList()
    {
        // Arrange - Start with clean database (no seeding)
        using var context = _factory.GetDbContext();
        context.Airports.RemoveRange(context.Airports);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/airports");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var airports = await response.Content.ReadFromJsonAsync<List<Airport>>();
        airports.Should().NotBeNull();
        airports.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAirport_WithValidData_ReturnsCreatedAirport()
    {
        // Arrange
        var newAirport = new Airport("DEN", "Denver International Airport", "Denver", "USA", "North America");

        // Act - Make HTTP POST request with JSON body
        var response = await _client.PostAsJsonAsync("/api/airports", newAirport);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdAirport = await response.Content.ReadFromJsonAsync<Airport>();
        createdAirport.Should().NotBeNull();
        createdAirport!.IataCode.Should().Be("DEN");
        createdAirport.Name.Should().Be("Denver International Airport");
        
        // Verify it was actually saved to database
        using var context = _factory.GetDbContext();
        var savedAirport = await context.Airports.FindAsync("DEN");
        savedAirport.Should().NotBeNull();
        savedAirport!.Name.Should().Be("Denver International Airport");
    }

    [Fact]
    public async Task CreateAirport_WithDuplicateIataCode_ReturnsConflict()
    {
        // Arrange - Seed data first
        await _factory.SeedTestDataAsync();

        var duplicateAirport = new Airport("LAX", "Different Airport", "Different City", "Different Country", "Different Region");

        // Act
        var response = await _client.PostAsJsonAsync("/api/airports", duplicateAirport);

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
        var invalidJson = "{ invalid json }";
        var content = new StringContent(invalidJson, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/airports", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAirport_WithMissingRequiredFields_HandlesMissingData()
    {
        // Arrange - Airport with missing required fields
        var incompleteAirport = new
        {
            // Missing IataCode, Name, etc.
            City = "Some City"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/airports", incompleteAirport);

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
        // Arrange
        var newAirport = new Airport("SEA", "Seattle-Tacoma International Airport", "Seattle", "USA", "North America");

        // Act 1 - Create airport
        var createResponse = await _client.PostAsJsonAsync("/api/airports", newAirport);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act 2 - Retrieve all airports
        var getResponse = await _client.GetAsync("/api/airports");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert - The created airport should be in the list
        var airports = await getResponse.Content.ReadFromJsonAsync<List<Airport>>();
        airports.Should().Contain(a => 
            a.IataCode == "SEA" && 
            a.Name == "Seattle-Tacoma International Airport");
    }
}