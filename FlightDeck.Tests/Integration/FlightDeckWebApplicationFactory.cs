using FlightDeck.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

            // Add in-memory database for testing
            services.AddDbContext<FlightDeckDbContext>(options =>
            {
                options.UseInMemoryDatabase("FlightDeckTestDb");
                // Disable sensitive data logging in tests for cleaner output
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
            new FlightDeck.Core.Models.Airport("LAX", "Los Angeles International", "Los Angeles", "USA", "North America"),
            new FlightDeck.Core.Models.Airport( "JFK", "John F. Kennedy International", "New York", "USA", "North America"),
            new FlightDeck.Core.Models.Airport("LHR", "London Heathrow", "London", "UK", "Europe")
        };

        context.Airports.AddRange(testAirports);
        await context.SaveChangesAsync();
    }
}