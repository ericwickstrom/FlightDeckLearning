using FlightDeck.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlightDeck.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        [HttpGet]
        public ActionResult<QuizQuestion> GetQuizQuestion()
        {
            // Get our airports (later this will come from database)
            var airports = new List<Airport>
            {
                new("ATL", "Hartsfield-Jackson Atlanta International", "Atlanta", "USA", "North America"),
                new("LAX", "Los Angeles International", "Los Angeles", "USA", "North America"),
                new("ORD", "O'Hare International", "Chicago", "USA", "North America"),
                new("JFK", "John F. Kennedy International", "New York", "USA", "North America")
            };

            // Pick a random airport for the question
            var random = new Random();
            var correctAirport = airports[random.Next(airports.Count)];

            // Create wrong answers from other airports
            var wrongAnswers = airports
                .Where(a => a.IataCode != correctAirport.IataCode)
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
}
