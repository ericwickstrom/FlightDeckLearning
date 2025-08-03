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
    public DbSet<User> Users { get; set; } // Add Users table

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Airport configuration
        modelBuilder.Entity<Airport>(entity =>
        {
            entity.HasKey(e => e.IataCode);
            entity.Property(e => e.IataCode).HasMaxLength(3);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Region).HasMaxLength(100);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);

            // Email must be unique
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // UserProgress configuration
        modelBuilder.Entity<UserProgress>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AirportCode).HasMaxLength(3);

            // Foreign key relationship
            entity.HasOne<User>()
                .WithMany(u => u.ProgressRecords)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}