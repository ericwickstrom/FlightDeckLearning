using System.ComponentModel.DataAnnotations;

namespace FlightDeck.Core.Models;

/// <summary>
/// User entity for authentication and progress tracking
/// </summary>
public class User
{
    public int Id { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime LastLoginAt { get; set; }
    
    // Navigation property for user progress
    public List<UserProgress> ProgressRecords { get; set; } = new();
}

/// <summary>
/// Data Transfer Objects for Authentication
/// </summary>
public record RegisterRequest(
    string Email,
    string Username, 
    string Password
);

public record LoginRequest(
    string Email,
    string Password
);

public record AuthResponse(
    string Token,
    string Email,
    string Username,
    DateTime ExpiresAt
);

public record UserInfo(
    int Id,
    string Email,
    string Username,
    DateTime CreatedAt,
    int TotalQuizzes,
    double AccuracyRate
);