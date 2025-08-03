using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightDeck.Core.Models;

public class UserProgress
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string AirportCode { get; set; } = string.Empty;
    
    [Range(0, int.MaxValue, ErrorMessage = "Correct answers cannot be negative")]
    public int CorrectAnswers { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Total attempts must be at least 1")]
    public int TotalAttempts { get; set; }
    
    [Required]
    public DateTime LastStudied { get; set; }
    
    // 🆕 Streak tracking
    [Range(0, int.MaxValue)]
    public int CurrentStreak { get; set; } = 0;
    
    [Range(0, int.MaxValue)]
    public int BestStreak { get; set; } = 0;
    
    // Computed properties - these don't get stored in database
    [NotMapped]
    public double AccuracyRate => TotalAttempts > 0 ? (double)CorrectAnswers / TotalAttempts : 0.0;
    
    [NotMapped]
    public bool IsWeak => AccuracyRate < 0.6;
}