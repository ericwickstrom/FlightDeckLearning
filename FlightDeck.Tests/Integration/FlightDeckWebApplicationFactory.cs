using FlightDeck.Infrastructure.Data;
using FlightDeck.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace FlightDeck.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for integration testing.
/// This creates a test version of your entire web application.
/// </summary>
public class FlightDeckWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real database registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<FlightDeckDbContext>));
            
            if (descriptor != null)
                services.Remove(descriptor);

            // Add in-memory database for testing with unique name per instance
            services.AddDbContext<FlightDeckDbContext>(options =>
            {
                options.UseInMemoryDatabase($"FlightDeckTestDb_{Guid.NewGuid()}");
                options.EnableSensitiveDataLogging(false);
            });

            // Build service provider and ensure database is created
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FlightDeckDbContext>();
            
            // Ensure the database is created
            context.Database.EnsureCreated();
        });

        // Use test environment
        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Helper method to get a fresh database context for test setup/cleanup
    /// </summary>
    public FlightDeckDbContext GetDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<FlightDeckDbContext>();
    }

    /// <summary>
    /// Helper method to seed test data
    /// </summary>
    public async Task SeedTestDataAsync()
    {
        using var context = GetDbContext();
        
        // Clear existing data
        context.Airports.RemoveRange(context.Airports);
        await context.SaveChangesAsync();

        // Add test airports
        var testAirports = new[]
        {
            new Airport("LAX", "Los Angeles International", "Los Angeles", "USA", "North America"),
            new Airport("JFK", "John F. Kennedy International", "New York", "USA", "North America"),
            new Airport("LHR", "London Heathrow", "London", "UK", "Europe")
        };

        context.Airports.AddRange(testAirports);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// 🆕 Helper method to create a test user and get JWT token
    /// </summary>
    public async Task<(string token, User user)> CreateTestUserAndTokenAsync()
    {
        var client = CreateClient();
        
        // Register a test user
        var registerRequest = new RegisterRequest(
            Email: "test@example.com",
            Username: "testuser",
            Password: "TestPassword123!"
        );

        var response = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        
        // Get the user from database using the same scope as the registration
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FlightDeckDbContext>();
        var user = await context.Users.FirstAsync(u => u.Email == "test@example.com");
        
        return (authResponse!.Token, user);
    }

    /// <summary>
    /// 🆕 Helper method to create authenticated HTTP client
    /// </summary>
    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var (token, _) = await CreateTestUserAndTokenAsync();
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}