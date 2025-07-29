using FlightDeck.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightDeck.Infrastructure.Data;

public class FlightDeckDbContext : DbContext
{
    public FlightDeckDbContext(DbContextOptions<FlightDeckDbContext> options) : base(options)
    {
    }

    public DbSet<Airport> Airports { get; set; }
    public DbSet<UserProgress> UserProgress { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Airport entity
        modelBuilder.Entity<Airport>(entity =>
        {
            entity.HasKey(e => e.IataCode);
            entity.Property(e => e.IataCode).HasMaxLength(3);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Region).HasMaxLength(50);
        });

        // Configure UserProgress entity
        modelBuilder.Entity<UserProgress>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.AirportCode });
        });

        // Seed data
        modelBuilder.Entity<Airport>().HasData(
            new Airport("ATL", "Hartsfield-Jackson Atlanta International", "Atlanta", "USA", "North America"),
            new Airport("LAX", "Los Angeles International", "Los Angeles", "USA", "North America"),
            new Airport("ORD", "O'Hare International", "Chicago", "USA", "North America"),
            new Airport("JFK", "John F. Kennedy International", "New York", "USA", "North America"),
            new Airport("DFW", "Dallas/Fort Worth International", "Dallas", "USA", "North America"),
            new Airport("DEN", "Denver International", "Denver", "USA", "North America"),
            new Airport("LAS", "McCarran International", "Las Vegas", "USA", "North America"),
            new Airport("PHX", "Phoenix Sky Harbor International", "Phoenix", "USA", "North America"),
            new Airport("MIA", "Miami International", "Miami", "USA", "North America"),
            new Airport("SEA", "Seattle-Tacoma International", "Seattle", "USA", "North America")
        );
    }
}