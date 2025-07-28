using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightDeck.Core.Models
{
    public record UserProgress(
        string UserId,
        string AirportCode,
        int CorrectAnswers,
        int TotalAttempts,
        DateTime LastStudied
    )
    {
        public double AccuracyRate => TotalAttempts > 0 ? (double)CorrectAnswers / TotalAttempts : 0.0;

        public bool IsWeak => AccuracyRate < 0.6; // Less than 60% accuracy
    }
}
