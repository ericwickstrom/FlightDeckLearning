using FlightDeck.Core.Models;
using FlightDeck.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlightDeck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // 🔐 Now requires authentication!
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

    // 🆕 NEW: Submit quiz answer and get feedback
    [HttpPost("answer")]
    public async Task<ActionResult<QuizAnswerResponse>> SubmitAnswer(QuizAnswerRequest request)
    {
        // Get the current user's ID from the JWT token
        var userIdClaim = User.FindFirst("userId")?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized("Invalid user token.");
        }

        // Find the airport being asked about
        var airport = await _context.Airports
            .FirstOrDefaultAsync(a => a.IataCode == request.QuestionCode);

        if (airport == null)
        {
            return BadRequest($"Airport code '{request.QuestionCode}' not found.");
        }

        // Determine if the answer is correct
        bool isCorrect = request.QuestionType switch
        {
            QuestionType.CodeToAirport =>
                string.Equals(request.UserAnswer.Trim(), airport.Name.Trim(), StringComparison.OrdinalIgnoreCase),
            QuestionType.AirportToCode =>
                string.Equals(request.UserAnswer.Trim(), airport.IataCode.Trim(), StringComparison.OrdinalIgnoreCase),
            _ => false
        };

        // Update or create user progress for this airport
        var progress = await _context.UserProgress
            .FirstOrDefaultAsync(up => up.UserId == userId && up.AirportCode == request.QuestionCode);

        if (progress == null)
        {
            // First time studying this airport
            progress = new UserProgress
            {
                UserId = userId,
                AirportCode = request.QuestionCode,
                CorrectAnswers = isCorrect ? 1 : 0,
                TotalAttempts = 1,
                CurrentStreak = isCorrect ? 1 : 0,
                BestStreak = isCorrect ? 1 : 0,
                LastStudied = DateTime.UtcNow
            };
            _context.UserProgress.Add(progress);
        }
        else
        {
            // Update existing progress
            progress.TotalAttempts++;
            progress.LastStudied = DateTime.UtcNow;

            if (isCorrect)
            {
                progress.CorrectAnswers++;
                progress.CurrentStreak++;

                // Update best streak if current streak is better
                if (progress.CurrentStreak > progress.BestStreak)
                {
                    progress.BestStreak = progress.CurrentStreak;
                }
            }
            else
            {
                // Reset current streak on wrong answer
                progress.CurrentStreak = 0;
            }
        }

        await _context.SaveChangesAsync();

        // Generate feedback message
        var correctAnswer = request.QuestionType switch
        {
            QuestionType.CodeToAirport => airport.Name,
            QuestionType.AirportToCode => airport.IataCode,
            _ => "Unknown"
        };

        var feedback = isCorrect
            ? $"Correct! ✅ Great job!"
            : $"Incorrect. ❌ The correct answer is: {correctAnswer}";

        if (isCorrect && progress.CurrentStreak > 1)
        {
            feedback += $" Streak: {progress.CurrentStreak}! 🔥";
        }

        // Create user stats
        var userStats = new QuizStats(
            TotalAttempts: progress.TotalAttempts,
            CorrectAnswers: progress.CorrectAnswers,
            AccuracyRate: Math.Round(progress.AccuracyRate * 100, 1), // Convert to percentage
            CurrentStreak: progress.CurrentStreak,
            BestStreak: progress.BestStreak
        );

        var response = new QuizAnswerResponse(
            IsCorrect: isCorrect,
            CorrectAnswer: correctAnswer,
            Feedback: feedback,
            UserStats: userStats
        );

        return Ok(response);
    }

    // 🆕 FIXED: Get current user's quiz stats with null safety
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetUserQuizStats()
    {
        // Get the current user's ID from the JWT token
        var userIdClaim = User.FindFirst("userId")?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized("Invalid user token.");
        }

        // Get user's progress records
        var progressRecords = await _context.UserProgress
            .Where(up => up.UserId == userId)
            .ToListAsync();

        var totalQuizzes = progressRecords.Sum(p => p.TotalAttempts);
        var totalCorrect = progressRecords.Sum(p => p.CorrectAnswers);
        var accuracyRate = totalQuizzes > 0 ? (double)totalCorrect / totalQuizzes : 0.0;
        var currentStreak = progressRecords.Sum(p => p.CurrentStreak);

        // FIXED: Handle empty collection case
        var bestStreak = progressRecords.Any() ? progressRecords.Max(p => p.BestStreak) : 0;

        var stats = new
        {
            TotalQuizzes = totalQuizzes,
            TotalCorrect = totalCorrect,
            AccuracyRate = Math.Round(accuracyRate * 100, 1), // Convert to percentage
            AirportsStudied = progressRecords.Count,
            WeakAirports = progressRecords.Where(p => p.AccuracyRate < 0.6).Count(),
            CurrentStreak = currentStreak,
            BestStreak = bestStreak
        };

        return Ok(stats);
    }
}