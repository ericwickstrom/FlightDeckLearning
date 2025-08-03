using FlightDeck.Core.Models;
using FlightDeck.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlightDeck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AirportsController : ControllerBase
{
    private readonly FlightDeckDbContext _context;

    public AirportsController(FlightDeckDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Airport>>> GetAirports()
    {
        var airports = await _context.Airports.ToListAsync();
        return Ok(airports);
    }

    [HttpPost]
    public async Task<ActionResult<Airport>> CreateAirport(Airport airport)
    {
        try
        {
            // Check if airport already exists
            var existingAirport = await _context.Airports
                .FirstOrDefaultAsync(a => a.IataCode == airport.IataCode);

            if (existingAirport != null)
            {
                return Conflict($"Airport with code '{airport.IataCode}' already exists.");
            }

            // Add the new airport
            _context.Airports.Add(airport);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetAirports), airport);
        }
        catch (DbUpdateException ex)
        {
            // This catches database-level constraint violations
            if (ex.InnerException?.ToString().Contains("duplicate") == true || 
                ex.InnerException?.ToString().Contains("UNIQUE") == true)
            {
                return Conflict($"Airport with code '{airport.IataCode}' already exists.");
            }
            
            // Re-throw if it's a different kind of database error
            throw;
        }
    }
}