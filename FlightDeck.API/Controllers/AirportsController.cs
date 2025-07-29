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
            _context.Airports.Add(airport);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAirports), airport);
        }
        catch (DbUpdateException)
        {
            // Fallback in case the check above somehow missed it
            return Conflict($"Airport with code '{airport.IataCode}' already exists.");
        }
    }
}