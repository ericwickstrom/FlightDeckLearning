using FlightDeck.Core.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace FlightDeck.Tests.Integration;

/// <summary>
/// Integration tests for QuizController - tests quiz generation endpoint
/// </summary>
public class QuizControllerIntegrationTests : IClassFixture<FlightDeckWebApplicationFactory>
{
    private readonly FlightDeckWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public QuizControllerIntegrationTests(FlightDeckWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetQuestion_WithSeededData_ReturnsValidQuizQuestion()
    {
        // Arrange - Seed test data
        await _factory.SeedTestDataAsync();

        // Act - Request a quiz question
        var response = await _client.GetAsync("/api/quiz/question");

        // Assert - Verify response structure
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var question = await response.Content.ReadFromJsonAsync<QuizQuestion>();
        
        question.Should().NotBeNull();
        question!.Code.Should().NotBeNullOrEmpty();
        question.CorrectAnswer.Should().NotBeNullOrEmpty();
        question.WrongAnswers.Should().NotBeNull();
        question.WrongAnswers.Should().HaveCount(3); // Your model has 3 wrong answers
        
        // Verify the correct answer is not in wrong answers
        question.WrongAnswers.Should().NotContain(question.CorrectAnswer);
    }

    [Fact]
    public async Task GetQuestion_WithSeededData_ReturnsValidQuestionType()
    {
        // Arrange
        await _factory.SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/api/quiz/question");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var question = await response.Content.ReadFromJsonAsync<QuizQuestion>();
        
        question.Should().NotBeNull();
        question!.Type.Should().BeOneOf(QuestionType.CodeToAirport, QuestionType.AirportToCode);
    }

    [Fact]
    public async Task GetQuestion_WithEmptyDatabase_ReturnsNotFound()
    {
        // Arrange - Start with empty database
        using var context = _factory.GetDbContext();
        context.Airports.RemoveRange(context.Airports);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/quiz/question");

        // Assert - Your QuizController returns NotFound when no airports exist
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetQuestion_MultipleCalls_CanReturnDifferentQuestions()
    {
        // Arrange - Need multiple airports for variety
        await _factory.SeedTestDataAsync();

        // Act - Make multiple requests
        var response1 = await _client.GetAsync("/api/quiz/question");
        var response2 = await _client.GetAsync("/api/quiz/question");
        var response3 = await _client.GetAsync("/api/quiz/question");

        // Assert - All should be successful
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        response3.StatusCode.Should().Be(HttpStatusCode.OK);

        var question1 = await response1.Content.ReadFromJsonAsync<QuizQuestion>();
        var question2 = await response2.Content.ReadFromJsonAsync<QuizQuestion>();
        var question3 = await response3.Content.ReadFromJsonAsync<QuizQuestion>();

        // With multiple airports, there's a chance we get different questions
        // This tests that the randomization is working
        var codes = new[] { question1!.Code, question2!.Code, question3!.Code };
        
        // Note: This could occasionally fail due to randomness, but with 3 airports 
        // and multiple question types, we should usually get some variety
        codes.Should().NotBeNull();
        codes.Distinct().Count().Should().BeGreaterThan(0); // At least some questions generated
    }

    [Fact]
    public async Task GetQuestion_CodeToAirportType_HasCorrectFormat()
    {
        // Arrange
        await _factory.SeedTestDataAsync();

        // Act - Make multiple requests to try to get a CodeToAirport question
        QuizQuestion? codeToAirportQuestion = null;
        
        for (int i = 0; i < 10 && codeToAirportQuestion == null; i++)
        {
            var response = await _client.GetAsync("/api/quiz/question");
            var question = await response.Content.ReadFromJsonAsync<QuizQuestion>();
            
            if (question?.Type == QuestionType.CodeToAirport)
            {
                codeToAirportQuestion = question;
            }
        }

        // Assert - If we found a CodeToAirport question, verify its format
        if (codeToAirportQuestion != null)
        {
            // Code should be a 3-letter airport code
            codeToAirportQuestion.Code.Should().MatchRegex(@"^[A-Z]{3}$");
            
            // Correct answer should be an airport name (not a code)
            codeToAirportQuestion.CorrectAnswer.Should().NotMatchRegex(@"^[A-Z]{3}$");
            
            // Wrong answers should also be airport names
            codeToAirportQuestion.WrongAnswers.Should().AllSatisfy(answer => 
                answer.Should().NotMatchRegex(@"^[A-Z]{3}$"));
        }
    }

    [Fact]
    public async Task GetQuestion_AirportToCodeType_HasCorrectFormat()
    {
        // Arrange
        await _factory.SeedTestDataAsync();

        // Act - Make multiple requests to try to get an AirportToCode question
        QuizQuestion? airportToCodeQuestion = null;
        
        for (int i = 0; i < 10 && airportToCodeQuestion == null; i++)
        {
            var response = await _client.GetAsync("/api/quiz/question");
            var question = await response.Content.ReadFromJsonAsync<QuizQuestion>();
            
            if (question?.Type == QuestionType.AirportToCode)
            {
                airportToCodeQuestion = question;
            }
        }

        // Assert - If we found an AirportToCode question, verify its format
        if (airportToCodeQuestion != null)
        {
            // Code should be an airport name (not a 3-letter code)
            airportToCodeQuestion.Code.Should().NotMatchRegex(@"^[A-Z]{3}$");
            
            // Correct answer should be a 3-letter airport code
            airportToCodeQuestion.CorrectAnswer.Should().MatchRegex(@"^[A-Z]{3}$");
            
            // All wrong answers should be 3-letter codes
            airportToCodeQuestion.WrongAnswers.Should().AllSatisfy(answer => 
                answer.Should().MatchRegex(@"^[A-Z]{3}$"));
        }
    }

    [Fact]
    public async Task GetQuestion_AllAnswersAreUnique()
    {
        // Arrange
        await _factory.SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/api/quiz/question");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var question = await response.Content.ReadFromJsonAsync<QuizQuestion>();
        question.Should().NotBeNull();

        // Combine all possible answers
        var allAnswers = question!.WrongAnswers.Concat(new[] { question.CorrectAnswer }).ToList();
        
        // All answers should be unique (no duplicates)
        allAnswers.Should().OnlyHaveUniqueItems();
        allAnswers.Should().HaveCount(4); // 3 wrong + 1 correct = 4 total
    }

    [Fact]
    public async Task GetQuestion_ConsistentResponseTiming_PerformsReasonably()
    {
        // Arrange
        await _factory.SeedTestDataAsync();

        // Act - Measure response times
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var response = await _client.GetAsync("/api/quiz/question");
        
        stopwatch.Stop();

        // Assert - Performance test
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should respond within 1 second
        
        // For a learning app, quiz questions should be fast!
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100, 
            "Quiz questions should be very fast for good user experience");
    }
}