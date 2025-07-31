using FlightDeck.Core.Models;
using FlightDeck.Infrastructure.Data;
using FlightDeck.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace FlightDeck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly FlightDeckDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthController(FlightDeckDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email || u.Username == request.Username);

            if (existingUser != null)
            {
                return Conflict("User with this email or username already exists.");
            }

            // Hash the password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create new user
            var user = new User
            {
                Email = request.Email,
                Username = request.Username,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);
            var expiresAt = _jwtService.GetTokenExpiration();

            var response = new AuthResponse(
                Token: token,
                Email: user.Email,
                Username: user.Username,
                ExpiresAt: expiresAt
            );

            return CreatedAtAction(nameof(GetUserInfo), new { id = user.Id }, response);
        }
        catch (DbUpdateException)
        {
            return Conflict("User with this email or username already exists.");
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        // Find user by email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid email or password.");
        }

        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Generate JWT token
        var token = _jwtService.GenerateToken(user);
        var expiresAt = _jwtService.GetTokenExpiration();

        var response = new AuthResponse(
            Token: token,
            Email: user.Email,
            Username: user.Username,
            ExpiresAt: expiresAt
        );

        return Ok(response);
    }

    [HttpGet("user/{id}")]
    public async Task<ActionResult<UserInfo>> GetUserInfo(int id)
    {
        var user = await _context.Users
            .Include(u => u.ProgressRecords)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        var totalQuizzes = user.ProgressRecords.Sum(p => p.TotalAttempts);
        var totalCorrect = user.ProgressRecords.Sum(p => p.CorrectAnswers);
        var accuracyRate = totalQuizzes > 0 ? (double)totalCorrect / totalQuizzes : 0.0;

        var userInfo = new UserInfo(
            Id: user.Id,
            Email: user.Email,
            Username: user.Username,
            CreatedAt: user.CreatedAt,
            TotalQuizzes: totalQuizzes,
            AccuracyRate: accuracyRate
        );

        return Ok(userInfo);
    }
}