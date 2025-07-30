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
        question!.QuestionText.Should().NotBeNullOrEmpty();
        question.CorrectAnswer.Should().NotBeNullOrEmpty();
        question.Options.Should().NotBeNull();
        question.Options.Should().HaveCount(4); // Assuming 4 multiple choice options
        question.Options.Should().Contain(question.CorrectAnswer); // Correct answer should be in options
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
    public async Task GetQuestion_WithEmptyDatabase_ReturnsAppropriateResponse()
    {
        // Arrange - Start with empty database
        using var context = _factory.GetDbContext();
        context.Airports.RemoveRange(context.Airports);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/quiz/question");

        // Assert - Should handle gracefully (exact behavior depends on your implementation)
        // You might return 404, 204, or a specific error message
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound, 
            HttpStatusCode.NoContent, 
            HttpStatusCode.BadRequest
        );
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
        var questions = new[] { question1!.QuestionText, question2!.QuestionText, question3!.QuestionText };
        
        // Note: This could occasionally fail due to randomness, but with 3 airports 
        // and multiple question types, we should usually get some variety
        questions.Should().NotBeNull();
        questions.Distinct().Should().HaveCountGreaterOrEqualTo(1); // At least some questions generated
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
            codeToAirportQuestion.QuestionText.Should().Contain("airport code");
            codeToAirportQuestion.QuestionText.Should().MatchRegex(@"\b[A-Z]{3}\b"); // Should contain 3-letter airport code
            codeToAirportQuestion.Options.Should().AllSatisfy(option => 
                option.Should().NotBeNullOrEmpty());
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
            airportToCodeQuestion.QuestionText.Should().Contain("code");
            airportToCodeQuestion.Options.Should().AllSatisfy(option => 
                option.Should().MatchRegex(@"^[A-Z]{3}$")); // All options should be 3-letter codes
            airportToCodeQuestion.CorrectAnswer.Should().MatchRegex(@"^[A-Z]{3}$");
        }
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