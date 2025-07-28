using FlightDeck.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlightDeck.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AirportsController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<Airport>> GetAirports()
    {
        // Temporary hardcoded data - we'll replace this with a database later
        var airports = new List<Airport>
        {
            new("ATL", "Hartsfield-Jackson Atlanta International", "Atlanta", "USA", "North America"),
            new("LAX", "Los Angeles International", "Los Angeles", "USA", "North America"),
            new("ORD", "O'Hare International", "Chicago", "USA", "North America"),
            new("JFK", "John F. Kennedy International", "New York", "USA", "North America")
        };

        return Ok(airports);
    }
}