using FlightDeck.Infrastructure.Data;
using FlightDeck.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace FlightDeck.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for integration testing.
/// This creates a test version of your entire web application.
/// </summary>
public class FlightDeckWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"FlightDeckTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real database registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<FlightDeckDbContext>));
            
            if (descriptor != null)
                services.Remove(descriptor);

            // Add in-memory database for testing with consistent name per factory instance
            services.AddDbContext<FlightDeckDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.EnableSensitiveDataLogging(false);
            });
        });

        // 🔧 FIX: Configure JWT settings for testing
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test-specific configuration
            var testConfig = new Dictionary<string, string>
            {
                ["Jwt:SecretKey"] = "FlightDeck-Test-JWT-Secret-Key-2025-Must-Be-At-Least-256-Bits-Long-For-Security",
                ["Jwt:Issuer"] = "FlightDeckTestIssuer",
                ["Jwt:Audience"] = "FlightDeckTestAudience",
                ["Jwt:ExpirationHours"] = "24"
            };

            config.AddInMemoryCollection(testConfig!);
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
    /// Helper method to reset database to clean state
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        using var context = GetDbContext();
        
        // Clear all data
        context.UserProgress.RemoveRange(context.UserProgress);
        context.Users.RemoveRange(context.Users);
        context.Airports.RemoveRange(context.Airports);
        await context.SaveChangesAsync();
        
        // Clear change tracker to avoid tracking issues
        context.ChangeTracker.Clear();
    }

    /// <summary>
    /// Helper method to seed test data
    /// </summary>
    public async Task SeedTestDataAsync()
    {
        using var context = GetDbContext();
        
        // Clear existing data first
        await ResetDatabaseAsync();

        // Add test airports (need at least 4 for quiz generation)
        var testAirports = new[]
        {
            new Airport("LAX", "Los Angeles International", "Los Angeles", "USA", "North America"),
            new Airport("JFK", "John F. Kennedy International", "New York", "USA", "North America"),
            new Airport("LHR", "London Heathrow", "London", "UK", "Europe"),
            new Airport("ATL", "Hartsfield-Jackson Atlanta International", "Atlanta", "USA", "North America")
        };

        context.Airports.AddRange(testAirports);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
    }

    /// <summary>
    /// Helper method to create a test user and get JWT token
    /// </summary>
    public async Task<(string token, User user)> CreateTestUserAndTokenAsync()
    {
        // First, ensure we have a clean state for user creation
        using (var setupContext = GetDbContext())
        {
            // Remove any existing test users
            var existingUsers = setupContext.Users.Where(u => u.Email.StartsWith("test"));
            setupContext.Users.RemoveRange(existingUsers);
            await setupContext.SaveChangesAsync();
        }

        var client = CreateClient();
        
        // Create unique email to avoid conflicts
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var registerRequest = new RegisterRequest(
            Email: $"test{uniqueId}@example.com",
            Username: $"testuser{uniqueId}",
            Password: "TestPassword123!"
        );

        var response = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to register test user: {response.StatusCode} - {error}");
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        
        if (authResponse?.Token == null)
        {
            throw new InvalidOperationException("Failed to get token from registration response");
        }

        // Get the user from database
        using var context = GetDbContext();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == registerRequest.Email);
        
        if (user == null)
        {
            throw new InvalidOperationException($"User not found after registration: {registerRequest.Email}");
        }
        
        return (authResponse.Token, user);
    }

    /// <summary>
    /// Helper method to create authenticated HTTP client
    /// </summary>
    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var (token, _) = await CreateTestUserAndTokenAsync();
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Clean up the in-memory database
            try
            {
                using var context = GetDbContext();
                context.Database.EnsureDeleted();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        base.Dispose(disposing);
    }
}