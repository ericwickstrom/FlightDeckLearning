using FlightDeck.Core.Models;
using FlightDeck.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlightDeck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly FlightDeckDbContext _context;

    public QuizController(FlightDeckDbContext context)
    {
        _context = context;
    }

    [HttpGet("question")]
    public async Task<ActionResult<QuizQuestion>> GetQuizQuestion()
    {
        // Get all airports from database
        var airports = await _context.Airports.ToListAsync();

        if (airports.Count < 4)
        {
            return BadRequest("Need at least 4 airports in database to generate quiz questions.");
        }

        // Pick a random airport for the question
        var random = new Random();
        var correctAirport = airports[random.Next(airports.Count)];

        // Create wrong answers from other airports
        var wrongAnswers = airports
            .Where(a => a.IataCode != correctAirport.IataCode)
            .OrderBy(x => random.Next())
            .Select(a => a.Name)
            .Take(3)
            .ToList();

        var quiz = new QuizQuestion(
            correctAirport.IataCode,
            correctAirport.Name,
            wrongAnswers,
            QuestionType.CodeToAirport
        );

        return Ok(quiz);
    }
}